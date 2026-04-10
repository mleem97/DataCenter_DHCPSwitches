using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace DHCPSwitches;

public static class DHCPManager
{
    private const string SUBNET_BASE = "192.168.1.";
    private const int POOL_START = 10;
    private const int POOL_END = 254;

    private static readonly HashSet<string> AssignedIPs = new();
    private static readonly Dictionary<int, string> CustomerSubnetPrefixCache = new();

    /// <summary>
    /// When true (default), empty <c>SetIP</c> from the game is auto-filled via Harmony (DHCP-style).
    /// Turn off from IPAM toolbar to stop background auto-assignment after clearing addresses.
    /// </summary>
    public static bool EmptyIpAutoFillEnabled { get; set; } = true;

    /// <summary>Skipped for the next empty <c>SetIP</c> when the mod intentionally clears an address.</summary>
    internal static bool SuppressEmptyIpAutoAssign { get; private set; }

    public static bool IsFlowPaused { get; private set; }

    /// <summary>Set when <see cref="SetServerIP"/> rejects or the game throws (shown in IPAM).</summary>
    public static string LastSetIpError { get; private set; }

    public static void ClearLastSetIpError() => LastSetIpError = null;

    public static void ClearCaches()
    {
        CustomerSubnetPrefixCache.Clear();
    }

    /// <summary>IPAM customer-assign and batch actions can surface errors the same way as <see cref="SetServerIP"/>.</summary>
    internal static void SetLastIpamError(string message) => LastSetIpError = message;

    public static void ToggleFlow()
    {
        IsFlowPaused = !IsFlowPaused;
        ModLogging.Msg(IsFlowPaused ? "Flow paused." : "Flow running.");
        ModDebugLog.Bootstrap();
        ModDebugLog.WriteLine(
            IsFlowPaused
                ? "IPAM: sim flow PAUSED — AddAppPerformance is blocked here."
                : "IPAM: sim flow RUNNING — AddAppPerformance prefix will run.");
    }

    public static void AssignAllServers()
    {
        if (!LicenseManager.IsDHCPUnlocked)
        {
            ModLogging.Warning("DHCP locked (toggle with Ctrl+D debug).");
            return;
        }

        var servers = UnityEngine.Object.FindObjectsOfType<Server>();
        RebuildAssignedIpsFromScene(servers);

        var assigned = 0;
        foreach (var server in servers)
        {
            var ip = GetServerIP(server);
            if (!string.IsNullOrWhiteSpace(ip) && ip != "0.0.0.0")
            {
                continue;
            }

            var newIp = GetNextFreeIpForServer(server, servers);
            if (string.IsNullOrEmpty(newIp))
            {
                continue;
            }

            if (SetServerIP(server, newIp, skipUsableListCheck: true))
            {
                AssignedIPs.Add(newIp);
                assigned++;
            }
        }

        if (assigned > 0)
        {
            ModLogging.Msg($"DHCP: {assigned} IPs assigned.");
            IPAMOverlay.InvalidateDeviceCache();
        }
    }

    /// <summary>Assign DHCP from the game's contract usable lists (and fallbacks) to every listed server; skips servers that already have an IP.</summary>
    public static int AssignDhcpToServers(System.Collections.Generic.IEnumerable<Server> servers)
    {
        if (servers == null)
        {
            return 0;
        }

        if (!LicenseManager.IsDHCPUnlocked)
        {
            ModLogging.Warning("DHCP locked (toggle with Ctrl+D debug).");
            return 0;
        }

        var allServers = UnityEngine.Object.FindObjectsOfType<Server>();
        RebuildAssignedIpsFromScene(allServers);
        var n = 0;
        foreach (var server in servers)
        {
            if (server == null)
            {
                continue;
            }

            var ip = GetServerIP(server);
            if (!string.IsNullOrWhiteSpace(ip) && ip != "0.0.0.0")
            {
                continue;
            }

            var newIp = GetNextFreeIpForServer(server, allServers);
            if (string.IsNullOrEmpty(newIp))
            {
                continue;
            }

            if (SetServerIP(server, newIp, skipUsableListCheck: true))
            {
                AssignedIPs.Add(newIp);
                n++;
            }
        }

        if (n > 0)
        {
            ModLogging.Msg($"DHCP: {n} server(s) assigned.");
            IPAMOverlay.InvalidateDeviceCache();
        }

        return n;
    }

    /// <summary>Assign one free usable address to a single server (toolbar / per-row DHCP auto).</summary>
    public static bool AssignDhcpToSingleServer(Server server)
    {
        if (!LicenseManager.IsDHCPUnlocked)
        {
            ModLogging.Warning("DHCP locked (toggle with Ctrl+D debug).");
            return false;
        }

        if (server == null)
        {
            return false;
        }

        var allServers = UnityEngine.Object.FindObjectsOfType<Server>();
        RebuildAssignedIpsFromScene(allServers);
        var newIp = GetNextFreeIpForServer(server, allServers);
        if (string.IsNullOrEmpty(newIp))
        {
            if (server.GetCustomerID() <= 0)
            {
                LastSetIpError = "DHCP failed: this server is not assigned to a customer contract.";
            }
            else
            {
                LastSetIpError = "DHCP failed: no free address was found in the customer subnet or legacy pool.";
            }

            ModLogging.Warning("DHCP: AssignDhcpToSingleServer found no free IP.");
            return false;
        }

        LastSetIpError = null;
        if (!SetServerIP(server, newIp, skipUsableListCheck: true))
        {
            return false;
        }

        IPAMOverlay.InvalidateDeviceCache();
        return true;
    }

    private static void RebuildAssignedIpsFromScene(IEnumerable<Server> servers)
    {
        AssignedIPs.Clear();
        if (servers == null)
        {
            return;
        }

        foreach (var s in servers)
        {
            if (s == null)
            {
                continue;
            }

            var existingIp = GetServerIP(s);
            if (!string.IsNullOrWhiteSpace(existingIp) && existingIp != "0.0.0.0")
            {
                AssignedIPs.Add(existingIp);
            }
        }
    }

    private static bool IsUnsetIp(string ip)
    {
        return string.IsNullOrWhiteSpace(ip) || ip == "0.0.0.0";
    }

    internal static string GetServerIP(Server server)
    {
        if (server == null)
        {
            return string.Empty;
        }

        return server.IP ?? string.Empty;
    }

    /// <param name="skipUsableListCheck">True when the address was chosen from the game's usable list (DHCP path).</param>
    /// <param name="suppressAutoAssignOnEmpty">True when clearing the address from IPAM so Harmony does not immediately assign a new IP.</param>
    internal static bool SetServerIP(
        Server server,
        string ip,
        bool skipUsableListCheck = false,
        bool suppressAutoAssignOnEmpty = false)
    {
        LastSetIpError = null;

        if (server == null)
        {
            return false;
        }

        var prevIp = GetServerIP(server);
        var needSuppress = suppressAutoAssignOnEmpty && IsUnsetIp(ip);
        try
        {
            if (needSuppress)
            {
                SuppressEmptyIpAutoAssign = true;
            }

            server.SetIP(ip);
            if (!string.IsNullOrWhiteSpace(prevIp) && prevIp != "0.0.0.0")
            {
                AssignedIPs.Remove(prevIp);
            }

            return true;
        }
        catch (Exception ex)
        {
            LastSetIpError = "The game rejected this address; see MelonLoader's log (e.g. MelonLoader/Latest.log) for details.";
            ModLogging.Error(ex);
            return false;
        }
        finally
        {
            if (needSuppress)
            {
                SuppressEmptyIpAutoAssign = false;
            }
        }
    }

    /// <summary>Legacy pool when no customer subnet is bound (e.g. main menu).</summary>
    private static string GetNextFreeLegacyPoolIp()
    {
        for (var i = POOL_START; i <= POOL_END; i++)
        {
            var candidate = SUBNET_BASE + i;
            if (!AssignedIPs.Contains(candidate))
            {
                return candidate;
            }
        }

        ModLogging.Warning("DHCP: legacy 192.168.1.x pool exhausted.");
        return null;
    }

    private static string GetNextFreeIpForServer(Server server, Server[] allServersCache = null)
    {
        if (server == null)
        {
            return GetNextFreeLegacyPoolIp();
        }

        var prefix = TryGetCustomerSubnetPrefix(server);
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            var customerIp = GetNextFreeIpFromPrefix(prefix);
            if (!string.IsNullOrWhiteSpace(customerIp))
            {
                return customerIp;
            }
        }

        return GetNextFreeLegacyPoolIp();
    }

    private static string TryGetCustomerSubnetPrefix(Server server)
    {
        if (server == null)
        {
            return null;
        }

        var customerId = server.GetCustomerID();
        if (customerId < 0)
        {
            return null;
        }

        if (CustomerSubnetPrefixCache.TryGetValue(customerId, out var cached))
        {
            return cached;
        }

        var prefix = TryLookupCustomerSubnetPrefix(customerId);
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            CustomerSubnetPrefixCache[customerId] = prefix;
            return prefix;
        }

        CustomerSubnetPrefixCache[customerId] = null;
        return null;
    }

    private static string TryLookupCustomerSubnetPrefix(int customerId)
    {
        foreach (var customer in UnityEngine.Object.FindObjectsOfType<CustomerBase>())
        {
            if (customer == null)
            {
                continue;
            }

            try
            {
                if (customer.customerID != customerId)
                {
                    continue;
                }
            }
            catch
            {
                continue;
            }

            var prefix = TryExtractCustomerSubnetPrefix(customer);
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                return prefix;
            }
        }

        return null;
    }

    private static string TryExtractCustomerSubnetPrefix(object customer)
    {
        if (customer == null)
        {
            return null;
        }

        var seen = new HashSet<object>();
        return TryExtractCustomerSubnetPrefixRecursive(customer, seen, 0);
    }

    private static string TryExtractCustomerSubnetPrefixRecursive(object obj, HashSet<object> seen, int depth)
    {
        if (obj == null || depth > 8)
        {
            return null;
        }

        if (obj is string strValue)
        {
            return TryParsePrefix(strValue);
        }

        var type = obj.GetType();
        if (type.IsPrimitive || type.IsEnum)
        {
            return null;
        }

        if (!seen.Add(obj))
        {
            return null;
        }

        if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
        {
            foreach (var item in enumerable)
            {
                var prefix = TryExtractCustomerSubnetPrefixRecursive(item, seen, depth + 1);
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    return prefix;
                }
            }
        }

        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            try
            {
                var fieldValue = field.GetValue(obj);
                if (fieldValue is string stringValue)
                {
                    var parsed = TryParsePrefix(stringValue);
                    if (!string.IsNullOrWhiteSpace(parsed))
                    {
                        return parsed;
                    }
                }

                var nested = TryExtractCustomerSubnetPrefixRecursive(fieldValue, seen, depth + 1);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
            catch
            {
            }
        }

        foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!prop.CanRead)
            {
                continue;
            }

            try
            {
                var propValue = prop.GetValue(obj);
                if (propValue is string stringValue)
                {
                    var parsed = TryParsePrefix(stringValue);
                    if (!string.IsNullOrWhiteSpace(parsed))
                    {
                        return parsed;
                    }
                }

                var nested = TryExtractCustomerSubnetPrefixRecursive(propValue, seen, depth + 1);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    return nested;
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static bool IsPrefixMemberName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        name = name.ToLowerInvariant();
        return name.Contains("subnet")
               || name.Contains("prefix")
               || name.Contains("network")
               || name.Contains("address")
               || name.Contains("ip")
               || name.Contains("gateway")
               || name.Contains("range")
               || name.Contains("cidr");
    }

    private static string TryParsePrefix(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var trimmed = rawValue.Trim();
        if (trimmed.EndsWith("/24", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 3).Trim();
        }

        if (trimmed.EndsWith(".0", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 2);
        }

        if (trimmed.EndsWith(".", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var parts = trimmed.Split('.');
        if (parts.Length == 4)
        {
            return $"{parts[0]}.{parts[1]}.{parts[2]}.";
        }

        return null;
    }

    private static string GetNextFreeIpFromPrefix(string prefixBase)
    {
        if (string.IsNullOrWhiteSpace(prefixBase))
        {
            return null;
        }

        if (!prefixBase.EndsWith("."))
        {
            prefixBase += ".";
        }

        for (var i = POOL_START; i <= POOL_END; i++)
        {
            var candidate = prefixBase + i;
            if (!AssignedIPs.Contains(candidate))
            {
                return candidate;
            }
        }

        ModLogging.Warning($"DHCP: pool exhausted for prefix {prefixBase}");
        return null;
    }

    [HarmonyPatch]
    public static class ServerSetIpPatch
    {
        public static MethodBase TargetMethod()
        {
            // Use typeof(Server) — avoid AccessTools.TypeByName, which scans every loaded assembly and spams
            // ReflectionTypeLoadException on IL2CPP Unity modules Harmony cannot load.
            var serverType = typeof(Server);
            return AccessTools.Method(serverType, "SetIP", new[] { typeof(string) })
                ?? AccessTools.Method(serverType, "AssignIP", new[] { typeof(string) })
                ?? AccessTools.Method(serverType, "SetAddress", new[] { typeof(string) });
        }

        public static void Prefix(object __instance, ref string _ip)
        {
            if (!LicenseManager.IsDHCPUnlocked)
            {
                return;
            }

            if (!EmptyIpAutoFillEnabled || SuppressEmptyIpAutoAssign)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(_ip) && _ip != "0.0.0.0")
            {
                return;
            }

            if (__instance is not Server server)
            {
                return;
            }

            // Physical / in-game "clear IP" passes empty or 0.0.0.0 while server.IP is still the old address.
            // Without this, we immediately DHCP again and the clear never sticks.
            var prevIp = GetServerIP(server);
            if (!IsUnsetIp(prevIp))
            {
                return;
            }

            var autoIp = GetNextFreeIpForServer(server, null);
            if (string.IsNullOrEmpty(autoIp))
            {
                return;
            }

            _ip = autoIp;
            AssignedIPs.Add(autoIp);
        }

        public static void Postfix(string _ip)
        {
            if (!string.IsNullOrWhiteSpace(_ip) && _ip != "0.0.0.0")
            {
                AssignedIPs.Add(_ip);
            }
        }
    }

    [HarmonyPatch]
    public static class FlowPausePatch
    {
        private static int _addAppPerformancePrefixHits;

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(CustomerBase), "AddAppPerformance");
        }

        public static bool Prefix(CustomerBase __instance)
        {
            var hit = Interlocked.Increment(ref _addAppPerformancePrefixHits);
            if (hit == 1)
            {
                ModDebugLog.WriteLine(
                    "IOPS: first AddAppPerformance call observed — Harmony gate is active (see IPAM Pause flow / DHCPSwitches-debug.log).");
            }

            var cid = __instance != null ? __instance.customerID : -1;
            if (IsFlowPaused)
            {
                ModDebugLog.WriteThrottledIopsDeny(
                    cid,
                    "FLOW_PAUSED: IPAM flow is paused — click Resume in IPAM so AddAppPerformance runs.");
                return false;
            }

            return true;
        }
    }
}

/// <summary>
/// Registered for Il2Cpp; periodic auto-assign was removed to avoid log spam and redundant work.
/// </summary>
public class DHCPController : MonoBehaviour
{
    public DHCPController(System.IntPtr ptr)
        : base(ptr)
    {
    }
}

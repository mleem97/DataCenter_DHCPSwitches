namespace DHCPSwitches.Tests;

public class UnitTest1
{
    [Theory]
    [InlineData("10.1.2.3", true)]
    [InlineData("172.16.0.1", true)]
    [InlineData("172.31.255.254", true)]
    [InlineData("172.32.0.1", false)]
    [InlineData("192.168.1.10", true)]
    [InlineData("8.8.8.8", false)]
    public void IsPrivateAddress_String_Recognizes_Rfc1918(string ip, bool expected)
    {
        var actual = Ipv4Rfc1918.IsPrivateAddress(ip);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("203.0.113.0/30", true)]
    [InlineData("198.51.100.0/31", true)]
    [InlineData("10.0.0.0/30", false)]
    [InlineData("192.168.10.0/31", false)]
    [InlineData("203.0.113.0/29", false)]
    public void LooksLikePublicPtpBlock_Validates_Cidr_Intent(string cidr, bool expected)
    {
        var actual = Ipv4Rfc1918.LooksLikePublicPtpBlock(cidr);
        Assert.Equal(expected, actual);
    }
}

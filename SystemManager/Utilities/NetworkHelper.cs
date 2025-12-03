using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SystemManager.Utilities
{
    /// <summary>
    /// Helper class for network operations
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// Get network adapters with signal strength
        /// </summary>
        public static List<NetworkAdapterInfo> GetNetworkAdapters()
        {
            var adapters = new List<NetworkAdapterInfo>();

            try
            {
                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var info = new NetworkAdapterInfo
                    {
                        Name = adapter.Name,
                        Description = adapter.Description,
                        Type = adapter.NetworkInterfaceType.ToString(),
                        Status = adapter.OperationalStatus.ToString(),
                        Speed = adapter.Speed,
                        MacAddress = adapter.GetPhysicalAddress().ToString()
                    };

                    var stats = adapter.GetIPStatistics();
                    info.BytesSent = stats.BytesSent;
                    info.BytesReceived = stats.BytesReceived;

                    adapters.Add(info);
                }
            }
            catch (Exception)
            {
                // Handle exceptions
            }

            return adapters;
        }

        /// <summary>
        /// Get local IP address
        /// </summary>
        public static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch
            {
                // Handle exceptions
            }

            return "127.0.0.1";
        }

        /// <summary>
        /// Get external IP address
        /// </summary>
        public static string GetExternalIPAddress()
        {
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString("https://api.ipify.org").Trim();
                }
            }
            catch
            {
                return "N/A";
            }
        }

        /// <summary>
        /// Get DNS servers
        /// </summary>
        public static List<string> GetDNSServers()
        {
            var dnsServers = new List<string>();

            try
            {
                foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up)
                    {
                        var properties = adapter.GetIPProperties();
                        foreach (var dns in properties.DnsAddresses)
                        {
                            if (!dnsServers.Contains(dns.ToString()))
                            {
                                dnsServers.Add(dns.ToString());
                            }
                        }
                    }
                }
            }
            catch
            {
                // Handle exceptions
            }

            return dnsServers;
        }

        /// <summary>
        /// Ping a host and measure latency
        /// </summary>
        public static PingResult PingHost(string host)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(host, 1000);
                    return new PingResult
                    {
                        Success = reply.Status == IPStatus.Success,
                        RoundtripTime = reply.RoundtripTime,
                        Status = reply.Status.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new PingResult
                {
                    Success = false,
                    Status = ex.Message
                };
            }
        }

        /// <summary>
        /// Get active TCP connections
        /// </summary>
        public static List<ConnectionInfo> GetActiveTCPConnections()
        {
            var connections = new List<ConnectionInfo>();

            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var tcpConnections = properties.GetActiveTcpConnections();

                foreach (var connection in tcpConnections)
                {
                    connections.Add(new ConnectionInfo
                    {
                        LocalEndPoint = connection.LocalEndPoint.ToString(),
                        RemoteEndPoint = connection.RemoteEndPoint.ToString(),
                        State = connection.State.ToString(),
                        Protocol = "TCP"
                    });
                }
            }
            catch
            {
                // Handle exceptions
            }

            return connections;
        }
    }

    public class NetworkAdapterInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public long Speed { get; set; }
        public string MacAddress { get; set; }
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
    }

    public class PingResult
    {
        public bool Success { get; set; }
        public long RoundtripTime { get; set; }
        public string Status { get; set; }
    }

    public class ConnectionInfo
    {
        public string LocalEndPoint { get; set; }
        public string RemoteEndPoint { get; set; }
        public string State { get; set; }
        public string Protocol { get; set; }
    }
}

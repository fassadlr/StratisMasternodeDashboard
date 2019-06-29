namespace Stratis.FederatedSidechains.AdminDashboard.Settings
{
    public class DefaultEndpointsSettings
    {
        public string StratisNode { get; set; }
        public string SidechainNode { get; set; }
        public string SidechainNodeType { get; set; }
        public string EnvType { get; set; }
    }

    public class NodeTypes
    {
        public const string TenK = "10K";
        public const string FiftyK = "50K";
    }

    public class NodeEnv
    {
        public const string TestNet = "TestNet";
        public const string MainNet = "MainNet";
    }
}
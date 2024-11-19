namespace RemTestWall.Model
{
    public class WallInfo
    {
        public long WallId { get; set; }
        public string ModelName { get; set; }
        public long? LinkInstanceId { get; set; }
        public string CategoryName => "Wall";
    }
}


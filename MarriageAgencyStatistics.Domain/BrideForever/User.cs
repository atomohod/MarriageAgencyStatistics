namespace MarriageAgencyStatistics.Domain.BrideForever
{
    public class User
    {
        public string Name { get; set; }
        public string ID { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
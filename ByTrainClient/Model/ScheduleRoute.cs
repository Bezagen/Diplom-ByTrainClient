namespace ByTrainClient.Model
{
    public class ScheduleRoute
    {
        public string DepartureCity { get; set; }
        public string DestinationCity { get; set; }
        public string DateAndTimeOfDeparture { get; set; }
        public string DateAndTimeOfArrival { get; set; }
        public int Train_ID { get; set; }
        public int AvailableSeats { get; set; }
    }
}

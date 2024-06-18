
namespace ByTrainClient.Model
{
    public class CertainRoute
    {
        public string DeparturePoint { get; set; }
        public string DestinationPoint { get; set; }
        public string DateTimeArrival { get; set; }
        public string DateTimeDeparture { get; set; }
        public int TrainID { get; set; }
        public int AvailableSeats { get; set; }
    }
}

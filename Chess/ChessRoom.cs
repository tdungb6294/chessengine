using Chess;

namespace ChessGame
{
    public class ChessRoom : IDisposable
    {
        public static int totalChessRooms = 0;
        static ChessRoom()
        {
            totalChessRooms = 0;
        }
        public string roomId { get; set; }
        public string roomName { get; set; }
        public List<Player> players { get; set; }
        public ChessRoom(string roomId, string roomName)
        {
            this.roomId = roomId;
            this.roomName = roomName;
            players = new List<Player>();
            totalChessRooms++;
        }
        ~ChessRoom()
        {
            Console.WriteLine($"Chess room with id {roomId} has been destroyed!");
        }

        public void Dispose()
        {
            totalChessRooms--;
        }
    }
}
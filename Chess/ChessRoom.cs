using Chess;

namespace ChessGame
{
    public class ChessRoom
    {
        public string roomId { get; set; }
        public string roomName { get; set; }
        public List<Player> players { get; set; }
        public ChessRoom(string roomId, string roomName)
        {
            this.roomId = roomId;
            this.roomName = roomName;
            players = new List<Player>();
        }
    }
}
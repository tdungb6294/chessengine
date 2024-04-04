using ChessGame;
using Chess;
using Microsoft.AspNetCore.SignalR;

public class ChessHub : Hub
{
    public static string HubUrl = "/chessrooms";
    private static List<ChessRoom> rooms = new List<ChessRoom>();

    public async Task<string> CreateRoom(string roomName)
    {
        if (rooms.Count > 10)
        {
            return "";
        }
        string roomId = Guid.NewGuid().ToString();
        ChessRoom chessRoom = new ChessRoom(roomId, roomName);
        rooms.Add(chessRoom);
        await Clients.All.SendAsync("ReceiveRoom", chessRoom);
        return roomId;
    }

    public async Task JoinRoom(string roomId, string playerName)
    {

        var room = rooms.FirstOrDefault(r => r.roomId == roomId);
        if (room != null && room.players.Count < 2)
        {
            var existingPlayer = room.players.FirstOrDefault<Player>(player => player.username == playerName);
            if (existingPlayer is not null) return;
            Player player;
            PlayerColor playerColor = PlayerColor.WHITE;
            if (room.players.Count == 1)
            {
                switch (room.players[0].playerColor)
                {
                    case PlayerColor.WHITE:
                        playerColor = PlayerColor.BLACK;
                        break;
                    case PlayerColor.BLACK:
                        playerColor = PlayerColor.WHITE;
                        break;
                }
            }
            player = new Player(playerName, playerColor);
            room.players.Add(player);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            Console.WriteLine($"{playerName} has connected to room {roomId}");
            await Clients.Group(roomId).SendAsync("RoomUpdated", room);
        }
    }

    public async Task LeaveRoom(string roomId, string playerName)
    {
        var room = rooms.FirstOrDefault(r => r.roomId == roomId);
        if (room != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            var player = room.players.FirstOrDefault(player => player.username == playerName);
            if (player is not null)
            {
                room.players.Remove(player);
            }
            Console.WriteLine($"{playerName} has disconnected from room {roomId}");
            await Clients.Group(roomId).SendAsync("RoomUpdated", room);
        }
    }

    public List<ChessRoom> GetRooms()
    {
        return rooms;
    }

    public ChessRoom? IsInRoom(string playerName, string roomId)
    {
        var room = rooms.FirstOrDefault(r => r.roomId == roomId);

        if (room is not null)
        {
            var player = room.players.FirstOrDefault(player => player.username == playerName);
            if (player is not null)
            {
                return room;
            }
        }
        return null;
    }


}
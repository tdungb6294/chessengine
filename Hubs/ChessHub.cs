using ChessGame;
using Chess;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

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
        await Clients.All.SendAsync("ReceiveRoom", rooms);
        return roomId;
    }

    public async Task JoinRoom(string roomId, string playerName)
    {

        var room = rooms.FirstOrDefault(r => r.roomId == roomId);
        if (room != null && room.players.Count < 2)
        {
            var existingPlayer = room.players.FirstOrDefault<Player>(player => player.contextId == Context.ConnectionId);
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
            player = new Player(playerName, playerColor, Context.ConnectionId);
            room.players.Add(player);
            Console.WriteLine($"{Context.ConnectionId} and username: {player.username} has connected to room {roomId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("RoomUpdated", room);
            //await Clients.Group(roomId).SendAsync("BoardUpdated", JsonConvert.SerializeObject(room.board));
            await Clients.All.SendAsync("ReceiveRoom", rooms);
        }
    }

    public async Task LeaveRoom(string roomId)
    {
        var room = rooms.FirstOrDefault(r => r.roomId == roomId);
        if (room != null)
        {
            Player? player = room.players.FirstOrDefault(player => player.contextId == Context.ConnectionId);
            if (player is not null)
            {
                room.players.Remove(player);
            }
            Console.WriteLine($"{Context.ConnectionId} and username: {player?.username} has disconnected from room {roomId}");
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                await Clients.Group(roomId).SendAsync("RoomUpdated", room);
                await Clients.All.SendAsync("ReceiveRoom", rooms);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }

    public List<ChessRoom> GetRooms()
    {
        return rooms;
    }

    public ChessRoom? IsInRoom(string roomId)
    {
        var room = rooms.FirstOrDefault(r => r.roomId == roomId);

        if (room is not null)
        {
            var player = room.players.FirstOrDefault(player => player.contextId == Context.ConnectionId);
            if (player is not null)
            {
                return room;
            }
        }
        return null;
    }

    public async Task MakeMove(string roomId, int x, int y, int tX, int tY)
    {
        var room = rooms.FirstOrDefault(r => r.roomId == roomId);
        if (room is not null)
        {
            var player = room.players.FirstOrDefault(player => player.contextId == Context.ConnectionId);
            if (player is not null)
            {
                if (room.board.isWhiteTurn && player.playerColor == PlayerColor.WHITE)
                {
                    room.MakeMove(x, y, tX, tY);
                    await Clients.Group(roomId).SendAsync("BoardUpdated", JsonConvert.SerializeObject(room.board));
                }
                else if (!room.board.isWhiteTurn && player.playerColor == PlayerColor.BLACK)
                {
                    room.MakeMove(x, y, tX, tY);
                    await Clients.Group(roomId).SendAsync("BoardUpdated", JsonConvert.SerializeObject(room.board));
                }
            }
        }
    }

    public async Task<string> GetBoard(string roomId)
    {
        var room = rooms.FirstOrDefault(r => r.roomId == roomId);
        return JsonConvert.SerializeObject(room.board);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Find the room the client was in
        foreach (var room in rooms)
        {
            var player = room.players.FirstOrDefault(player => player.contextId == Context.ConnectionId);
            if (player is not null)
            {
                // Remove the client from the room
                room.players.Remove(player);
                await base.OnDisconnectedAsync(exception);
                await Clients.Group(room.roomId).SendAsync("RoomUpdated", room);
                await Clients.All.SendAsync("ReceiveRoom", rooms);
                break;
            }
        }
    }
}
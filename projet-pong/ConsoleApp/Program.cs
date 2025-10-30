using Core;
using System.Diagnostics;
using System.Text;

namespace ConsoleApp;


public class ConsoleInput : IInput
{
    private readonly HashSet<ConsoleKey> _heldKeys = new HashSet<ConsoleKey>();
    private bool _restart = false;
    private DateTime _lastKeyTime = DateTime.Now;

    public void Update()
    {
        // Lecture inputs  
        while (Console.KeyAvailable)
        {
            var keyInfo = Console.ReadKey(true);
            
            if (keyInfo.Key == ConsoleKey.R)
                _restart = true;
            else
                _heldKeys.Add(keyInfo.Key);
            
            _lastKeyTime = DateTime.Now;
        }

        // Main
        if ((DateTime.Now - _lastKeyTime).TotalMilliseconds > 100)
        {
            _heldKeys.Clear();
        }
    }

    public void Clear()
    {
        _restart = false;
    }

    public float GetPlayer1Direction()
    {
        if (_heldKeys.Contains(ConsoleKey.Z)) return -1;
        if (_heldKeys.Contains(ConsoleKey.S)) return 1;
        return 0;
    }

    public float GetPlayer2Direction()
    {
        if (_heldKeys.Contains(ConsoleKey.UpArrow)) return -1;
        if (_heldKeys.Contains(ConsoleKey.DownArrow)) return 1;
        return 0;
    }

    public bool ShouldRestart() => _restart;
}


// Rendu ASCII

public class ConsoleRenderer
{
    private readonly int _width;
    private readonly int _height;
    private readonly float _scaleX;
    private readonly float _scaleY;
    private readonly char[,] _buffer;
    private readonly StringBuilder _sb;

    public ConsoleRenderer(int width, int height, float arenaWidth, float arenaHeight)
    {
        _width = width;
        _height = height;
        _scaleX = width / arenaWidth;
        _scaleY = height / arenaHeight;
        _buffer = new char[_height, _width];
        _sb = new StringBuilder((_width + 1) * _height + 200);
    }

    public void Render(GameState state)
    {
        
        Console.SetCursorPosition(0, 0);
        _sb.Clear();

        // clear buffer
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
                _buffer[y, x] = ' ';

        // Bordures haut et bas
        for (int x = 0; x < _width; x++)
        {
            _buffer[0, x] = '═';
            _buffer[_height - 1, x] = '═';
        }

        // Ligne centrale
        int centerX = _width / 2;
        for (int y = 1; y < _height - 1; y++)
        {
            if (y % 2 == 0)
                _buffer[y, centerX] = '|';
        }

        // Dessiner les objets
        DrawPaddle(state.Player1);
        DrawPaddle(state.Player2);

        DrawBall(state.Ball);

        // Construire la sortie dans le StringBuilder
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                _sb.Append(_buffer[y, x]);
            }
            _sb.AppendLine();
        }

        // Interface
        _sb.AppendLine();
        _sb.AppendLine("  ╔═══════════════════════════════════════════════════════╗");
        _sb.Append("  ║  Player 1: ");
        _sb.Append(state.ScorePlayer1.ToString().PadLeft(2));
        _sb.Append("  vs  Player 2: ");
        _sb.Append(state.ScorePlayer2.ToString().PadLeft(2));
        _sb.AppendLine("                       ║");
        _sb.AppendLine("  ╚═══════════════════════════════════════════════════════╝");

        if (state.IsGameOver)
        {
            _sb.AppendLine();
            string winner = state.ScorePlayer1 > state.ScorePlayer2 ? "PLAYER 1" : "PLAYER 2";
            _sb.Append("       ");
            _sb.Append(winner);
            _sb.AppendLine(" WINS!     ");
            _sb.AppendLine("  Press [R] to restart");
        }
        else
        {
            _sb.AppendLine();
            _sb.AppendLine("  Controls: [Z/S] Player 1  |  [up/down] Player 2");
        }

        // Write all
        Console.Write(_sb.ToString());
    }

    private void DrawPaddle(Paddle paddle)
    {
        int x = (int)(paddle.Position.X * _scaleX);
        int startY = (int)(paddle.Position.Y * _scaleY);
        int endY = (int)((paddle.Position.Y + paddle.Height) * _scaleY);

        for (int y = startY; y <= endY && y < _height; y++)
        {
            if (y >= 0 && x >= 0 && x < _width)
                _buffer[y, x] = '█';
        }
    }

    private void DrawBall(Ball ball)
    {
        int x = (int)(ball.Position.X * _scaleX);
        int y = (int)(ball.Position.Y * _scaleY);

        if (y >= 0 && y < _height && x >= 0 && x < _width)
            _buffer[y, x] = 'O';
    }
}


/// Programme principal

class Program
{
    static void Main()
    {
        // Configuration
        const float ARENA_WIDTH = 800f;
        const float ARENA_HEIGHT = 600f;
        const int CONSOLE_WIDTH = 80;
        const int CONSOLE_HEIGHT = 20; 
        const int TARGET_FPS = 30; 
        const float FRAME_TIME = 1f / TARGET_FPS;

        // Configuration console
       
            Console.CursorVisible = false;
            Console.SetWindowSize(CONSOLE_WIDTH + 1, CONSOLE_HEIGHT + 10);
            Console.SetBufferSize(CONSOLE_WIDTH + 1, CONSOLE_HEIGHT + 10);
       

        Console.Clear();
        Console.Title = "PONG - Console";

        // Affichage de démarrage
        Console.WriteLine("\n\n  ╔═══════════════════════════════════════╗");
        Console.WriteLine("  ║                                       ║");
        Console.WriteLine("  ║          P O N G   G A M E            ║");
        Console.WriteLine("  ║                                       ║");
        Console.WriteLine("  ║     First to 5 points wins!           ║");
        Console.WriteLine("  ║                                       ║");
        Console.WriteLine("  ╚═══════════════════════════════════════╝");
        Console.WriteLine("\n  Press any key to start...");
        Console.ReadKey(true);
        Console.Clear();

        // Initialisation
        var input = new ConsoleInput();
        var game = new PongGame(ARENA_WIDTH, ARENA_HEIGHT, input);
        var renderer = new ConsoleRenderer(CONSOLE_WIDTH, CONSOLE_HEIGHT, ARENA_WIDTH, ARENA_HEIGHT);

        // loop
        var stopwatch = Stopwatch.StartNew();
        float accumulator = 0f;
        int frameCount = 0;

        while (true)
        {
            float deltaTime = (float)stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            accumulator += deltaTime;

            // Mise à jour 
            while (accumulator >= FRAME_TIME)
            {
                input.Update();
                game.Update(FRAME_TIME);
                input.Clear();
                accumulator -= FRAME_TIME;
            }

            // ralenti pour eviter clignotement
            frameCount++;
            if (frameCount % 2 == 0) 
            {
                renderer.Render(game.State);
            }
            Thread.Sleep(33); 
            
        }
    }
}
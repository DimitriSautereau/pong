namespace Core;

// Structure mathematique
public struct Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vector2 Zero => new Vector2(0, 0);
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 v, float s) => new Vector2(v.X * s, v.Y * s);

    public float Length() => MathF.Sqrt(X * X + Y * Y);
}


// Interface controles

public interface IInput
{
    float GetPlayer1Direction(); // -1 up, 1 = down
    float GetPlayer2Direction();
    bool ShouldRestart();
}


// paddles
public class Paddle
{
    public Vector2 Position { get; set; }
    public float Width { get; }
    public float Height { get; }
    public float Speed { get; }

    public Paddle(Vector2 position, float width, float height, float speed)
    {
        Position = position;
        Width = width;
        Height = height;
        Speed = speed;
    }

    public void Move(float direction, float deltaTime, float minY, float maxY)
    {
        float newY = Position.Y + direction * Speed * deltaTime;
        newY = Math.Clamp(newY, minY, maxY - Height);
        Position = new Vector2(Position.X, newY);
    }
}

// Balle
public class Ball
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Radius { get; }

    public Ball(Vector2 position, float radius)
    {
        Position = position;
        Radius = radius;
        Velocity = Vector2.Zero;
    }

    public void Update(float deltaTime)
    {
        Position += Velocity * deltaTime;
    }
}

/// GameSate 
public class GameState
{
    public Paddle Player1 { get; }
    public Paddle Player2 { get; }
    public Ball Ball { get; }
    public float ArenaWidth { get; }
    public float ArenaHeight { get; }
    public int ScorePlayer1 { get; private set; }
    public int ScorePlayer2 { get; private set; }
    public bool IsGameOver { get; private set; }
    public int WinningScore { get; }

    public GameState(float arenaWidth, float arenaHeight, int winningScore = 5)
    {
        ArenaWidth = arenaWidth;
        ArenaHeight = arenaHeight;
        WinningScore = winningScore;

        // Configuration
        float paddleWidth = 20f;
        float paddleHeight = 100f;
        float paddleSpeed = 400f;
        float ballRadius = 10f;
        float paddleOffset = 50f;

        // Initialisation paddles, balle
        Player1 = new Paddle(
            new Vector2(paddleOffset, arenaHeight / 2 - paddleHeight / 2),
            paddleWidth, paddleHeight, paddleSpeed
        );

        Player2 = new Paddle(
            new Vector2(arenaWidth - paddleOffset - paddleWidth, arenaHeight / 2 - paddleHeight / 2),
            paddleWidth, paddleHeight, paddleSpeed
        );

        Ball = new Ball(new Vector2(arenaWidth / 2, arenaHeight / 2), ballRadius);
    }

    public void AddPointPlayer1()
    {
        ScorePlayer1++;
        CheckGameOver();
    }
    
    public void AddPointPlayer2()
    {
        ScorePlayer2++;
        CheckGameOver();
    }

    public void Reset()
    {
        ScorePlayer1 = 0;
        ScorePlayer2 = 0;
        IsGameOver = false;
    }

    private void CheckGameOver()
    {
        if (ScorePlayer1 >= WinningScore || ScorePlayer2 >= WinningScore)
        {
            IsGameOver = true;
        }
    }
}


// physique du jeu
public class PhysicsSystem
{
    private readonly Random _random = new Random(); 
    private const float INITIAL_SPEED = 300f;
    private const float SPEED_INCREASE = 1.05f;
    private const float MAX_BOUNCE_ANGLE = 60f;

    public void Update(GameState state, float deltaTime)
    {
        // Mise à jour de la balle
        state.Ball.Update(deltaTime);

        // Collisions : murs haut/bas
        CheckWallCollisions(state);

        // Collisions : raquettes
        CheckPaddleCollisions(state);

        // Point marqué
        CheckScoring(state);
    }

    //Début de manche 
    public void LaunchBall(GameState state)
    {
        if (state.Ball.Velocity.Length() == 0)
        {
            float angle = (_random.NextSingle() - 0.5f) * 60f * MathF.PI / 180f;
            float direction = _random.Next(2) == 0 ? -1 : 1;

            float velX = direction * INITIAL_SPEED * MathF.Cos(angle);
            float velY = INITIAL_SPEED * MathF.Sin(angle);

            state.Ball.Velocity = new Vector2(velX, velY);
        }
    }

    private void CheckWallCollisions(GameState state)
    {
        Ball ball = state.Ball;

        // Collision haut
        if (ball.Position.Y - ball.Radius <= 0)
        {
            ball.Position = new Vector2(ball.Position.X, ball.Radius);
            ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
        }
        // Collision bas
        else if (ball.Position.Y + ball.Radius >= state.ArenaHeight)
        {
            ball.Position = new Vector2(ball.Position.X, state.ArenaHeight - ball.Radius);
            ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
        }
    }
    
    // Collision joueur 1 ou joueur 2
    private void CheckPaddleCollisions(GameState state)
    {
        CheckPaddleCollision(state, state.Player1, -1);
        
        CheckPaddleCollision(state, state.Player2, 1);
    }

    private void CheckPaddleCollision(GameState state, Paddle paddle, float directionSign)
    {
        Ball ball = state.Ball;

        float paddleLeft = paddle.Position.X;
        float paddleRight = paddle.Position.X + paddle.Width;
        float paddleTop = paddle.Position.Y;
        float paddleBottom = paddle.Position.Y + paddle.Height;

        // Vérifier si la balle est dans la zone verticale de la raquette
        bool inVerticalRange = ball.Position.Y + ball.Radius >= paddleTop && 
                                ball.Position.Y - ball.Radius <= paddleBottom;

        // Vérifier si la balle touche la raquette horizontalement
        bool hitting = directionSign < 0
            ? (ball.Position.X - ball.Radius <= paddleRight && ball.Velocity.X < 0)
            : (ball.Position.X + ball.Radius >= paddleLeft && ball.Velocity.X > 0);

        if (inVerticalRange && hitting)
        {
            // Calculer l'angle de rebond basé sur où la balle touche la raquette
            float relativeIntersect = (ball.Position.Y - (paddleTop + paddle.Height / 2)) / (paddle.Height / 2);
            float bounceAngle = relativeIntersect * MAX_BOUNCE_ANGLE * MathF.PI / 180f;

            // Augmenter la vitesse
            float speed = ball.Velocity.Length() * SPEED_INCREASE;

            // Nouvelle vélocité
            float newVelX = -directionSign * speed * MathF.Cos(bounceAngle);
            float newVelY = speed * MathF.Sin(bounceAngle);

            ball.Velocity = new Vector2(newVelX, newVelY);

            // Repositionner pour éviter que la balle reste coincée
            if (directionSign < 0)
            {
                ball.Position = new Vector2(paddleRight + ball.Radius, ball.Position.Y);
            }
            else
            {
                ball.Position = new Vector2(paddleLeft - ball.Radius, ball.Position.Y);
            }
        }
    }

    private void CheckScoring(GameState state)
    {
        Ball ball = state.Ball;

        // gauche - Score joueur 2
        if (ball.Position.X - ball.Radius <= 0)
        {
            state.AddPointPlayer2();
            ResetBall(state);
        }
        // droite - Score joueur 1
        else if (ball.Position.X + ball.Radius >= state.ArenaWidth)
        {
            state.AddPointPlayer1();
            ResetBall(state);
        }
    }

    private void ResetBall(GameState state)
    {
        if (!state.IsGameOver)
        {
            state.Ball.Position = new Vector2(state.ArenaWidth / 2, state.ArenaHeight / 2);
            state.Ball.Velocity = Vector2.Zero;
        }
    }
}


// Jeu Pong - Entrypoint
// Boucle de jeu

public class PongGame
{
    public GameState State { get; }
    private readonly PhysicsSystem _physics;
    private readonly IInput _input;
    private bool _ballLaunched;

    public PongGame(float arenaWidth, float arenaHeight, IInput input, int winningScore = 5)
    {
        State = new GameState(arenaWidth, arenaHeight, winningScore);
        _physics = new PhysicsSystem();
        _input = input;
        _ballLaunched = false;
    }

   
    public void Update(float deltaTime)
    {
       
        if (_input.ShouldRestart() && State.IsGameOver)
        {
            Restart();
            return;
        }

        if (State.IsGameOver)
        {
            return;
        }

        // Début de manche
        if (!_ballLaunched && State.Ball.Velocity.Length() == 0)
        {
            _physics.LaunchBall(State);
            _ballLaunched = true;
        }

        // Mouvement Paddles
        State.Player1.Move(
            _input.GetPlayer1Direction(),
            deltaTime,
            0,
            State.ArenaHeight
        );

        State.Player2.Move(
            _input.GetPlayer2Direction(),
            deltaTime,
            0,
            State.ArenaHeight
        );

        _physics.Update(State, deltaTime);

        if (State.Ball.Velocity.Length() == 0)
        {
            _ballLaunched = false;
        }
    }

    public void Restart()
    {
        State.Reset();
        State.Ball.Position = new Vector2(State.ArenaWidth / 2, State.ArenaHeight / 2);
        State.Ball.Velocity = Vector2.Zero;
        _ballLaunched = false;
    }
}
using Core;
using Godot;
using GodotVector2 = Godot.Vector2; 
using CoreVector2 = Core.Vector2;     

namespace GodotApp;



public partial class PongController : Node2D
{
	// Références aux nodes visuels
	private ColorRect _player1Visual;
	private ColorRect _player2Visual;
	private ColorRect _ballVisual;
	private Label _scoreLabel;
	private Label _gameOverLabel;

	// Le jeu Core
	private PongGame _game;
	private GodotInput _input;

	// Constantes
	private const float ARENA_WIDTH = 800f;
	private const float ARENA_HEIGHT = 600f;

	public override void _Ready()
	{
		// Récupérer les nodes de la scène
		_player1Visual = GetNode<ColorRect>("Player1");
		_player2Visual = GetNode<ColorRect>("Player2");
		_ballVisual = GetNode<ColorRect>("Ball");
		_scoreLabel = GetNode<Label>("UI/ScoreLabel");
		_gameOverLabel = GetNode<Label>("UI/GameOverLabel");

		// Initialiser le jeu Core
		_input = new GodotInput();
		_game = new PongGame(ARENA_WIDTH, ARENA_HEIGHT, _input);

	}

	public override void _Process(double delta)
	{
		_game.Update((float)delta);
		SyncVisuals();
	}

	private void SyncVisuals(){
		
		GameState state = _game.State;

		// Convertir Core.Vector2 vers Godot.Vector2
		_player1Visual.Position = new GodotVector2(
			state.Player1.Position.X,
			state.Player1.Position.Y
		);

		_player2Visual.Position = new GodotVector2(
			state.Player2.Position.X,
			state.Player2.Position.Y
		);

		
		_ballVisual.Position = new GodotVector2(
			state.Ball.Position.X - state.Ball.Radius,
			state.Ball.Position.Y - state.Ball.Radius
		);

		// Score
		_scoreLabel.Text = $"{state.ScorePlayer1}  -  {state.ScorePlayer2}";

		// Game Over
		if (state.IsGameOver)
		{
			string winner = state.ScorePlayer1 > state.ScorePlayer2 ? "PLAYER 1" : "PLAYER 2";
			_gameOverLabel.Text = $"{winner} WINS!\nPress R to restart";
			_gameOverLabel.Visible = true;
		}
		else
		{
			_gameOverLabel.Visible = false;
		}
	}
}

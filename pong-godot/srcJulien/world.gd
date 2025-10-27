extends Node2D

@onready var ball = $Ball
@onready var score_label = $CanvasLayer/Label_Score

var score_p1 = 0
var score_p2 = 0

func _ready() -> void:
	ball.connect("scored", Callable(self, "_on_ball_scored"))
	_update_score_label()

func _process(delta: float) -> void:
	if Input.is_action_just_pressed("restart"):
		get_tree().reload_current_scene()

func _on_ball_scored(player_id: String) -> void:
	if player_id == "player1":
		score_p1 += 1
	elif player_id == "player2":
		score_p2 += 1
	_update_score_label()

func _update_score_label() -> void:
	score_label.text = str(score_p1, " - ", score_p2)

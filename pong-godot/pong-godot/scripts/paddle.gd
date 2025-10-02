extends Area2D


@export var is_player_one = false

var up_input = "paddle_up"
var down_input = "paddle_down"

const MAX_SPEED = 10.0
var velocity = 0.0
var acceleration = 100.0 

func ready():
	if is_player_one == false:
		up_input += "_two"
		down_input += "_two"

func _physics_process(delta: float) -> void:
	
	var move_dir = 0.0
	
	move_dir = Input.get_axis(up_input,down_input)
	
	velocity += move_dir * acceleration * delta
	
	
	if move_dir == 0:
		velocity = move_toward(velocity, 0, 5)
	
	velocity = clampf(velocity,-MAX_SPEED, MAX_SPEED) 
	
	global_position.y += velocity 
	
	print(velocity)
	

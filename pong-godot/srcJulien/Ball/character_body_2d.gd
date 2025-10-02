extends CharacterBody2D

const Speed := 5.0
var start_position := Vector2.ZERO
var can_move := true 

func _ready() -> void:
	# Sauvegarde la position de départ 
	start_position = position
	velocity = Vector2(-Speed, 0)

func _physics_process(delta: float) -> void:
	if can_move :
		var col := move_and_collide(velocity)
		if col:
			var collider = col.get_collider()
			var normal = col.get_normal()
			
			# Si on touche un mur lateral la balle respawn au centre
			if collider.is_in_group("mur") and normal.x != 0:
				respawn()
				return
			
			# Sinon rebond normal haut/bas
			velocity = velocity.bounce(normal)

func respawn():
	# Balle respawn au centre
	position = start_position
	can_move = false
	velocity = Vector2.ZERO
	
	start_after_delay()
	
func start_after_delay() -> void:
	await get_tree().create_timer(2.0).timeout
	var dir := Vector2(randf_range(-1, 1), randf_range(-1, 1)).normalized()
	velocity = dir * Speed
	can_move = true

extends CharacterBody2D

signal scored(player_id)

const Speed := 5.0
var start_position := Vector2.ZERO
var can_move := true 

func _ready() -> void:
	start_position = position
	velocity = Vector2(-Speed, 0)

func _physics_process(delta: float) -> void:
	if can_move:
		var col := move_and_collide(velocity)
		if col:
			var collider = col.get_collider()
			var normal = col.get_normal()

			# Si on touche un mur latéral
			if collider.is_in_group("mur") and normal.x != 0:
				# 🔁 Inversé : maintenant le joueur du côté touché marque
				if normal.x > 0:
					emit_signal("scored", "player2") # mur gauche touché → joueur gauche marque
				elif normal.x < 0:
					emit_signal("scored", "player1") # mur droit touché → joueur droit marque
				
				respawn()
				return

			# Sinon rebond normal haut/bas
			velocity = velocity.bounce(normal)

func respawn():
	position = start_position
	can_move = false
	velocity = Vector2.ZERO
	start_after_delay()

func start_after_delay() -> void:
	await get_tree().create_timer(2.0).timeout
	var dir := Vector2(randf_range(-1, 1), randf_range(-1, 1)).normalized()
	velocity = dir * Speed
	can_move = true

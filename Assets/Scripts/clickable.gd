# Clickable.gd
extends ColorRect

signal clicked(node)

func _ready():
	# Allow this Control to capture mouse events
	mouse_filter = MOUSE_FILTER_STOP
	# Optionally add to a group for batch-connections
	add_to_group("Clickable")

func _gui_input(event):
	# Check for left mouse button press
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		emit_signal("clicked", self)
 

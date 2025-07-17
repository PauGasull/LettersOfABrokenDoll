# GameManager.gd
extends Node

func _ready():
	# Connect all existing Clickable nodes using a Callable
	for clickable in get_tree().get_nodes_in_group("Clickable"):
		clickable.connect("clicked", self._on_clickable_clicked)

# If you add clickables at runtime, use this helper too
func connect_clickable(clickable):
	clickable.connect("clicked", self._on_clickable_clicked)

func _on_clickable_clicked(node):
	# Called when any ColorRect-Clickable emits "clicked"
	print("Clicked node: %s" % node.name)

# GameManager.gd
extends Node

var unread_letters: Array = []

func _ready():
	for clickable in get_tree().get_nodes_in_group("Clickable"):
		clickable.clicked.connect(self._on_clickable_clicked)
	LetterManager.letter_received.connect(self._on_letter_received)
	LetterManager.composition_prompt.connect(self._on_composition_prompt)
	LetterManager.letter_sent.connect(self._on_letter_sent)
	$CanvasLayer/PanelContainer/TextureRect/VBoxContainer/OptionInput.text_submitted.connect(self._on_OptionInput_text_submitted)

func connect_clickable(clickable):
	clickable.clicked.connect(self._on_clickable_clicked)

func _on_clickable_clicked(node):
	if node.name == "LetterClicker":
		_show_next_letter()
	elif node.name == "ComputerClicker":
		LetterManager.start_letter("COM001")
	print("Clicked node: %s" % node.name)

func _on_letter_received(text):
	unread_letters.append(text)
	$CanvasLayer/TextureRect/LetterClicker/LetterAlert.visible = true

func _on_letter_sent(text):
	$CanvasLayer/PanelContainer.visible = false

func _on_composition_prompt(prompt, options):
	var panel = $CanvasLayer/PanelContainer
	panel.visible = true
	panel.get_node("TextureRect/VBoxContainer/RichTextLabel").text = prompt
	panel.get_node("TextureRect/VBoxContainer/OptionInput").text = ""
	var keys = options.keys()
	keys.sort()
	var labels = [
		panel.get_node("TextureRect/VBoxContainer/RichTextLabel2"),
		panel.get_node("TextureRect/VBoxContainer/RichTextLabel3"),
		panel.get_node("TextureRect/VBoxContainer/RichTextLabel4")
	]
	for i in range(min(labels.size(), options.size())):
		labels[i].text = "%s: %s" % [keys[i], options[keys[i]]]

func _show_next_letter():
	if unread_letters.is_empty():
		return
	var letter = unread_letters.pop_front()
	print("READ LETTER:\n" + letter)
	if unread_letters.is_empty():
		$CanvasLayer/TextureRect/LetterClicker/LetterAlert.visible = false

func _on_OptionInput_text_submitted(new_text):
	LetterManager.choose_option(new_text.strip_edges().to_upper())
	$CanvasLayer/PanelContainer/TextureRect/VBoxContainer/OptionInput.text = ""

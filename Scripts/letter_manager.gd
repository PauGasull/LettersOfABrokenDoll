extends Node  # NO class_name, ja que és un autoload

signal composition_prompt(prompt: String, options: Dictionary)
signal letter_sent(text: String)
signal letter_received(text: String)

# Dades carregades des de JSON
var comp_data: Dictionary = {}
var letter_meta: Dictionary = {}
var resp_data: Dictionary = {}

# Estat durant la composició
var current_template: String
var current_block: String
var choice_path: Array = []
var player_letter: String = ""

# Històric de la conversa
var conversation: Array = []  # cada entrada: { type: "sent"/"received", text: String, timestamp: int }


func _ready() -> void:
	comp_data   = _load_json("res://Letters/composition.json")
	letter_meta = _load_json("res://Letters/letter.json")
	resp_data   = _load_json("res://Letters/responses.json")


func _load_json(path: String) -> Dictionary:
	var f = FileAccess.open(path, FileAccess.READ)
	var txt = f.get_as_text()
	f.close()
	var json = JSON.new()
	var err = json.parse(txt)
	if err != OK:
		push_error("Failed parsing %s: %d" % [path, err])
		return {}
	return json.data


func start_letter(template_id: String) -> void:
	current_template = template_id
	current_block    = comp_data[template_id].root_block
	choice_path.clear()
	player_letter = ""
	_emit_prompt()


func choose_option(option_id: String) -> void:
	var block = comp_data[current_template].blocks[current_block]
	# Afegim el long_text a la carta
	player_letter += block.options[option_id].long_text
	choice_path.append(option_id)
	# Passem al següent bloc
	current_block = block.options[option_id].next
	if current_block != null:
		_emit_prompt()
	else:
		send_letter()


func _emit_prompt() -> void:
	var block = comp_data[current_template].blocks[current_block]
	var opts: Dictionary = {}
	for id in block.options.keys():
		opts[id] = block.options[id].short_text
	emit_signal("composition_prompt", block.prompt, opts)


func send_letter() -> void:
	# Emetem la carta enviada
	emit_signal("letter_sent", player_letter)
	# Afegim a l’històric
	conversation.append({
		"type": "sent",
		"text": player_letter,
		"timestamp": Time.get_unix_time_from_system() as int
	})
	_schedule_response()


func _schedule_response() -> void:
	var delay = letter_meta[current_template].delay_seconds
	var timer = get_tree().create_timer(delay)
	# Connectem el timeout amb Callable
	timer.timeout.connect(Callable(self, "_on_response_timeout"))


func _on_response_timeout() -> void:
	# Construïm la clau del camí: "A_B_C"
	var key = ""
	for i in range(choice_path.size()):
		key += choice_path[i]
		if i < choice_path.size() - 1:
			key += "_"
	# Obtenim els blocs de resposta i concat en text
	var blocks = resp_data[current_template].paths[key].blocks
	var reply_text = ""
	for bid in blocks:
		reply_text += resp_data[current_template].responses[bid].content + "\n\n"
	reply_text = reply_text.strip_edges()
	# Històric i senyal
	conversation.append({
		"type": "received",
		"text": reply_text,
		"timestamp": Time.get_unix_time_from_system() as int
	})
	emit_signal("letter_received", reply_text)

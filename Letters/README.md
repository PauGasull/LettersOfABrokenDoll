# Project Overview

This project implements a branching letter-exchange mechanic between a player and the character **Ku’umi**, using three separate JSON documents:

1. **composition.json**: Defines the *composition templates* for the player’s letters, structured as mini decision trees.
2. **letter.json**: Specifies *per-letter metadata*, such as delivery delays.
3. **responses.json**: Contains the *response templates* for Ku’umi’s replies, mapping player choice paths to response blocks.

These files decouple data (text, branching logic, timing) from game code, enabling flexible story design without altering scripts.

---

## 1. composition.json

This file describes *how the player builds their letter*, step by step.

```jsonc
{
  "COM001": {
	"root_block": "P1",
	"blocks": {
	  "P1": {
		"prompt": "Start your letter by greeting Ku’umi:",
		"options": {
		  "A": {
			"short_text": "Hello Ku’umi,",
			"long_text": "Hello dear Ku’umi,\n",
			"next": "P2"
		  },
		  "B": { /* … */ }
		}
	  },
	  "P2": { /* next block… */ }
	}
  }
}
```

* **Top-level keys**: template IDs (e.g., `COM001`).
* **root\_block**: ID of the first block in the composition tree.
* **blocks**: Map of block IDs to objects containing:

  * `prompt`: Question or instruction shown to the player.
  * `options`: Map of option IDs (`A`, `B`, etc.) to:

	* `short_text`: Shown in the choice menu.
	* `long_text`: Appended to the player’s final letter when selected.
	* `next`: ID of the next block (or `null` if this ends the tree).

### How it’s used in code

1. Load and parse `composition.json`.
2. Start at `root_block`; show each `prompt` and the corresponding `short_text` options.
3. On player selection:

   * Record the option ID.
   * Append its `long_text` to the player-letter buffer.
   * Move to `next` block until `null`.
4. Return the sequence of choices *and* the full letter text.

---

## 2. letter.json

This file stores metadata *per composition template*, mainly delivery delays.

```jsonc
{
  "COM001": {
	"delay_seconds": 3600
  },
  "COM002": {
	"delay_seconds": 86400
  }
}
```

* **Top-level keys**: composition template IDs.
* **delay\_seconds**: Time (in seconds) between player sending the letter and Ku’umi’s response.

### How it’s used in code

1. After the player submits their letter, look up `delay_seconds` for the template.
2. Create a timer (e.g., `get_tree().create_timer(delay_seconds)`).
3. Upon timeout, generate and display Ku’umi’s response.

---

## 3. responses.json

This file defines *how Ku’umi replies*, based on the exact path of player choices.

```jsonc
{
  "COM001": {
	"paths": {
	  "A_A_A": { "blocks": ["R1","R2","R3"] },
	  "A_B_C": { "blocks": ["R4","R5"] }
	},
	"responses": {
	  "R1": { "content": "Hello again… I sense your tremble." },
	  "R2": { "content": "Can you tell me why you’re shaking?" },
	  "R3": { "content": "I’m here, listening closely." },
	  "R4": { "content": "That strange object glows faintly." }
	}
  }
}
```

* **paths**: Map of *concatenated choice sequences* (e.g., `"A_B_C"`) to:

  * `blocks`: *Ordered* list of response-block IDs.
* **responses**: Map of block IDs (`"R1"`, `"R2"`, etc.) to:

  * `content`: The text snippet to append in order.

### How it’s used in code

1. Receive the player’s choice path (e.g., `A_B_C`).
2. Look up `paths[pathKey].blocks` to get a list of response-block IDs.
3. Iterate the IDs, fetching each `responses[block_id].content`.
4. Concatenate and display as Ku’umi’s reply after the delay.

---

## Gameplay Flow Summary

1. **Load JSON data** (`composition.json`, `letter.json`, `responses.json`).
2. **Show initial letter** (if any).
3. **Compose player letter** via `composition.json`, gathering choice path & letter text.
4. **Schedule response** using `delay_seconds` from `letter.json`.
5. **Generate response** by mapping choice path to response blocks in `responses.json`.
6. **Display** Ku’umi’s reply.
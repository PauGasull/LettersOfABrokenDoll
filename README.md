# Letters of a Broken Doll

**Intrigue** · **Difficulty: Medium** · **Engine: Unity 6.1 (6000.1.12f1)** · **Status: In Progress**

> 💡 Can you help a distant friend escape certain death?

---

## 🎮 Description

"Letters of a Broken Doll" is a 2D interactive novel where you exchange letters with someone trapped in the middle of a civil war. Your choices shape the story, generate multiple endings, and unlock unique content based on your actions.

## ✨ Key Features

* **Modular letters**: freely write in optional sections and use placeholders to reference earlier paragraphs.
* **Handcrafted procedural generation** of letters to maximize narrative variability.
* **Dynamic news**: reading media outlets provides additional options based on your decisions.
* **Collectibles**: stamps that affect reading speed and vinyl records featuring piano improvisations.
* **Illustration attachments**: include drawings that influence how the story concludes.
* **Multiple endings** driven by emotional bonds and political actions.
* **Accessibility**: monospaced font choices, adjustable typing speed, and separate volume controls.

## 🛠 Project Structure

```
Assets/
├── Icons/            # Game icon, splash-screen, logos, etc...
├── Letters/          # JSONs files containing all letter information
├── Scenes/           # Unity scenes and some specific configuration if needed
├── Scripts/          # C# game logic, Custom editor scripts, and other utilities 
├── Settings/         # Global unity settings such as Renderer2D, Universal RP
├── Sprites/          # In-Game Graphics 
├── TextMesh Pro/     # Unity's Text Mesh Pro asset files 
└── Other files
```

## 📜 License

See [LICENSE](LICENSE.md) for details.

## 🤝 Credits

* **Pau Gasull** · Programmer
* **Roc “Ondo” Humet** · Writer
* **Bunny83** · [SimpleJson.cs](https://github.com/Bunny83/SimpleJSON/blob/master/SimpleJSON.cs)

964's C# Game Development Toolkit 

# **Document Version:** 1.2 

**Last Updated:** 2026 October 30 

# **Toolkit Version:** 0.0.0 

**Package ID:** com.964.gamedevelopmenttoolkit 

# **Purpose:** 

This is a collection of reusable game modules designed to reduce repetitive implementation work of common game features across multiple game projects. 

This project is for games created using the C# languages. While Unity-based games are able to use this project, the modules inside this toolkit are not solely for Unity projects. Any game projects, as long as built in C#, can use the modules of this toolkit. 

# **Credits:** 

Anyone who wants to use this project is allowed to use it without explicitly asking for my permission, as long as they properly credit me and this toolkit in their credits section. Even a special mention or acknowledgement is enough to credit me. Using this toolkit for your game project without disclosing that you utilized this toolkit is not permitted. All copyrights of this toolkit belong to me, but the copyright for the games that utilize this toolkit belong to their respective owners. 

# **Disclaimer:** 

I do not necessarily share any political, racial, gender, religious, and legal claims that developers using this toolkit may have. Additionally, games that handle sensitive issues and have any political, racial, gender, and religious themes are not in any way connected to me or my project. Usage of this toolkit by any developer does not necessarily mean I am connected to them in any way. 

# **Game Modules planned for development:** 

- **Username Module** 

This is a game module intended for games that require any usernames. Developers using this module can configure the maximum character length, the minimum character length (default of 1, can not be lower than that), and what special characters to allow. 

- **Timer Module** 

This module allows developers to configure timers for their game, it allows developers to create multiple timers for various purposes. Timers can be configured from milliseconds to days depending on the developer’s needs. 

The module also allows for timer duration changes for various reasons, like a timer increase or timer decrease based on any buff or debuff the player incurs. The module has methods which allow the developer to change the duration by the amount they state. This allows any +10secs and -5secs powerups in games to work. 

The module is more like a timer maker, if the developers have various timers, they can configure each differently, for example an internal timer for cooldowns or timers visible to the user like round timer. 

The module also allows for pause, the developer can configure which event pauses that timer. One example use case is in chess timer, where the move timer for one player can be paused. The module only contains a pause function that the developer calls and no logic on which calls the pause function, so the developer can configure it however they like. 

The module also contains a timer zero output, once the timer reaches zero, it produces a boolean output. Whether the user calls this or not is their choice, and what they want to do when the timer reaches zero is also their choice. 

- **Game Settings System** 

This is a system that the developer can use to make various configurable “Settings” for the player as they need. The developers can add “BGM”, “SFX”, key bindings, and whatever setting they need from the available settings in the module. 

- **Save Data System** 

This module is helpful for developers who want to have a save and load feature in their game. 

This module contains a proper save state feature. The developers can configure how many save states the players can have. The developers can also configure whether the player can save their game freely or upon certain game events only. This is done by calling the save method of the module. The save method contains the logic for saving the game but not how or when it should be called, so the developer is able to configure when the player is able to save. There’s also an option to allow the player to rename the save state which the developer can turn on. 

This module obviously includes a load state feature. The developers can configure whether loading a save state is an unlimited feature or there’s a hard load limit (like 3 loads of that save state then it’s deleted). The developers can also allow players whether to load freely or only on certain game events. 

This module does not handle persistent data. If the developers want to have any permanent data in their game that isn’t affected by any save state, they must configure that logic into their game themselves. 

- **Music Playlist System** 

This module is called “CSMusicPlayer”. It is an internal reference to the developer’s old Java project called “JMusicPlayer”. The name itself is a reference to Java features like JFrame and JPanel, which JMusicPlayer utilizes. 

This module allows the player to add their local music files into the game’s background music playlist. It has all the normal music player’s features like shuffle, loop, forward and backward, and favorite. It also allows developers to give players an option to make playlists. Depending on the module configuration, it also allows 

players to add built-in in-game music to their playlists along with their added local music. 

The module can read the tags inside the music to handle song name, song album, album image, etc.. The developers can also give the players the option to rename the song name, song album, and edit the album image if they want. 

The developers can also allow players to play specific songs or specific playlists for certain parts of the game. This includes the startup, main menu BGM, in-game BGM, boss music, and other stuff. Developers can also use this feature for their in-game music without allowing players to add local music. 

# **Limitations:** 

This project is solely for C# based games. It does not allow usage for other programming languages and other games that do not use C#. 

The modules do not contain any UI, they mainly provide data and function. Although some modules may have outputs that can be connected to in-game UI, that is something for the developers who want to use that module to handle. 

# **Guidelines:** 

- The core modules should avoid dependencies on Unity or other game engines whenever reasonably possible. Unity-specific functionality should be isolated into separate integration/adapter code when necessary. 

- Modules must not implement presentation/UI functionality. Modules should expose data, state, return values, and functionality that consuming games can connect to their own UI or presentation systems. 

- The preferred architecture is to keep reusable game logic in standard C# code with minimal external dependencies. Unity-specific integrations may be provided separately when required, but should not be required by the core implementation unless the functionality inherently depends on Unity. 

# **Design Principles:** 

- Prioritize reusability across multiple C# game projects. 

- Avoid unnecessary dependencies on specific game engines. 

- Do not implement UI or presentation. 

- Modules should expose data and functionality through standard C# types whenever practical. 

- Modules should be configurable rather than requiring source-code modification for individual games. 

- Game-specific behavior should be handled through configuration, parameters, interfaces, events, or extension points where appropriate. 

- Avoid unnecessary complexity; modules should solve focused, reusable problems. 


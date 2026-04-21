# DGD2003_EFE_CAN_ESER

GAME DESIGN DOCUMENT (GDD)
Project Name: The Panopticon Protocol
Genre: First-Person Psychological Horror / Stealth
Engine: Unity 3D
Target Platform: PC (Windows)
Author: Efe Can
Date: March 2026
Version: 1.0 (Final Submission)

1. GAME OVERVIEW
1.1. Logline
Trapped in a brutalist, cross-shaped school building during a midnight lockdown, a student must restore power to the main doors while avoiding "The Gaze"—an anomaly in the center of the school that constantly watches their every move.

1.2. Design Philosophy & Scope Note
To keep the scope realistic for this development cycle, the game relies heavily on environmental storytelling, stark lighting, and audio cues rather than complex enemy combat AI. The visual style is minimalist and low-poly, using shadows to hide the lack of high-resolution textures and to increase performance.

2. LEVEL DESIGN & ARCHITECTURE
The entire game takes place within a single, interconnected level modeled using Unity ProBuilder.

2.1. The Cross-Shaped Layout
The school is designed in a strict cross (cruciform) shape. This is intentional:

The Hexagonal Void: The exact center of the cross features a large hexagonal hole that drops straight down through all three floors.

Line of Sight: Because the hallways are straight, the player can see all the way to the end of a wing, but this also means they are exposed if they stand in the middle.

2.2. The Floors
Floor 1 (Ground Level): Contains the Main Exit (locked electronically) and Administration. It is completely pitch black.

Floor 2 (Classrooms): The middle layer. This is where the player starts. It consists of repetitive, symmetrical classroom doors, creating a maze-like disorientation.

Floor 3 (Archives & Power Room): The top layer. Cramped, filled with physical obstacles (overturned desks, bookshelves), and where the main objective is located.

3. CORE GAMEPLAY MECHANICS
3.1. Movement & Controls
The game is played in First-Person perspective to maximize immersion and claustrophobia.

W, A, S, D: Move

Left Shift: Sprint (Drains Stamina rapidly, makes loud footsteps)

Left Ctrl: Crouch (Slow movement, completely silent)

E: Interact (Pick up keys, open doors, pull levers)

3.2. The Main Threat: "The Gaze" (Core Mechanic)
Instead of a monster that walks around, the primary threat is a floating, mechanical eye-like anomaly located in the central Hexagonal Void.

Tracking System: Using a ScreenToWorldPoint and Raycast logic, The Gaze continuously tracks the player's position in 3D space. As long as the player is moving, the eye rotates to look at them.

The Sanity Meter: If the player steps into the central hexagonal area or stands in the open hallways without hiding behind a wall/pillar, The Gaze locks onto them. A red vignette effect slowly fills the screen. If the player does not break the line of sight within 5 seconds, it's an instant Game Over.

Gameplay Loop: The player must time their sprints across the central hexagonal gap, using overturned lockers and pillars as cover from the central tracking entity.

3.3. The Flashlight Management
The player starts with a weak flashlight.

Battery Drain: The flashlight drains over time.

The Catch: Turning the flashlight ON makes it easier to navigate the dark classrooms and find puzzle items, but it increases the detection speed of "The Gaze." The player must constantly toggle it (F key) to survive.

4. GAME PROGRESSION & OBJECTIVES
The game flow is structured as a linear sequence of tension-building fetch quests, designed to force the player to cross the dangerous central void multiple times.

Phase 1: Orientation. The player wakes up on Floor 2. The objective is to find a way out. They discover the central void and are introduced to The Gaze mechanic.

Phase 2: The Janitor's Keys. The player must sneak to the East Wing of Floor 2 to find the keys to the staircase. They must hide behind pillars to avoid the entity's tracking.

Phase 3: Restoring Power. The player unlocks the stairs, goes up to Floor 3, and navigates a maze of bookshelves to find the Breaker Room. They must find 3 missing fuses scattered in different rooms on Floor 3 to activate the breaker.

Phase 4: The Escape. Once power is restored, the school alarms go off. The Gaze becomes hyper-active, spinning rapidly. The player must make a final sprint all the way down to Floor 1's Main Exit before the anomaly catches them.

5. AUDIO & VISUAL DESIGN
5.1. Visuals
Lighting: Global illumination is extremely low. The primary light sources are emergency red lights in the corridors and the player's flashlight.

Skybox: The outside world (visible through small top-floor windows) uses a dark, moving storm skybox (Render Texture) to add to the oppressive atmosphere.

5.2. Audio
Ambient: No music. Only the low-frequency hum of the building's ventilation.

The Gaze: A heavy, mechanical grinding sound plays whenever the entity rotates to track the player. This gives the player crucial audio feedback that they are being watched, even if they aren't looking at the center.

6. ASSET & SCRIPT REQUIREMENTS
(Checklist for Development)

3D Models: ProBuilder cross-shaped level, basic locker models, fuse models.

Scripts: * PlayerMovement.cs (Standard FPS controller with crouch/sprint modifiers)

EyeFollowCursor.cs (Adapted to track the player's Transform instead of the mouse, handling the LookRotation logic)

InteractionSystem.cs (Raycast from the center of the camera to pick up fuses/keys)

SanityManager.cs (Handles the red vignette UI and Game Over trigger)

----------------------------------------------------------------------

This information is subject to change.

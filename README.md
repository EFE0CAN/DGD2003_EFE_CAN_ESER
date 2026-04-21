🏫 Project: School Anomaly Hunt
Developer: Efe Can

Department: Digital Game Design, İKÜ

📝 A Personal Note & Apology
First and foremost, I would like to sincerely apologize for the missing features and technical errors in this current version of the project.

Recently, I have been going through a very difficult period due to unexpected health challenges, which significantly impacted my productivity. Despite these hardships, I dedicated a vast amount of my time to the software and technical architecture of this game.

Note to Evaluator: I kindly invite you to examine the codebase and scripts. While some visual or gameplay features might be lacking, the underlying logic and system communication (Cinemachine blending, Event systems, and Physics-based interaction) represent the true depth of my effort. I appreciate your understanding and hope the technical dedication shines through the current shortcomings.

-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

🎮 Game Design Document (GDD)
1. Concept Overview
School Anomaly Hunt is a first/third-person psychological horror and puzzle game. The player takes on the role of a student/investigator tasked with clearing a school environment of supernatural anomalies.

2. Core Gameplay Loop
Explore: Navigate through school corridors and classrooms.

Identify: Use observation skills to find 3 specific anomalies in the environment.

Interact: Use the 'E' key to "cleanse" or interact with the anomalies.

Survival: All tasks must be completed within a strict Time Limit.

3. The "Fail State" (The Creature)
Time is your greatest enemy. If the player fails to identify and clear the anomalies before the timer hits zero:

A mysterious creature spawns within the school.

The creature hunts the player down, leading to an immediate Game Over.

4. Technical Features
Cinemachine System: Advanced camera switching (Follow, Security, and Zoom) using Custom Blending for a cinematic feel.

Script Communication: Heavy use of UnityEvents to trigger animations, sounds, and gameplay states without hard-coding dependencies.

Physics Interaction: Precise raycasting system for object detection and anomaly cleansing.
************************************************
Thank you for your time and evaluation. Efe Can

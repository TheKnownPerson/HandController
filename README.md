# HAND CONTROLLER

HandController is a freecam and hand control mod that is supposed to be used for modded rooms for the game Gorilla Tag.

It's a mod for PC!

![image](https://github.com/user-attachments/assets/02ced825-99d5-4141-a38e-4a24736a8e6c)
## Compatibility

this mod is not compatible with any mods that change your perspective on your desktop.
## What does it do?
This Mod adds: a Freecam, the possibility to use your mouse to click on buttons, Hand movement, Leave, Join codes, Generate a random private room, toggle comp lobbies on and off and it adds labels that display the roomcode and the playercount.

## Why is my UI not showing up?
Your UI not showing up could have several reasons following

reason 1: Your SteamVR probably is still trying to connect to the VR headset or is connected to it. How to fix it: Close SteamVR if your VR headset isn't connected.

reason 2: Your HideManagerGameObject is set to false Scroll down to find how to fix it(Why are my modded room options not showing up?)

## How do I open the UI?
Press the TAB key on your keyboard.

## Freecam Movement

W: Forward movement

S: Backward movement

D: Right movement

A: Left Movement


Hold your left mouse button and move it around to turn your perspective horizontally and vertically!

## Why can't I join non moddeds?

The mod doesn't allow you to join non moddeds because it's to prevent room hopping and cheating.
this mod only allows you to join non moddeds when it's disabled or when a vr is connected.

## Why do my modded room options not show up?

It's because the bepinex config automatically sets HideManagerGameObject to false, you wanna set that to true and I will guide you through it:

Step 1: Go to your steam library and press on gorilla tag.

Step 2: Click on the settings icon and do as shown in the picture

![image](https://github.com/user-attachments/assets/a68cc9b9-bb6f-41c9-9913-704c47b61142)

Step 3: Click on Browse local files in the Manage category

Step 4: Once you're in the Gorilla Tag folder look for the BepInEx folder

Step 5: If you found it click on it a config folder in the BepInEx folder should show up, click on it.

Step 6: Search for BepInEx.cfg if you found it open it up with a text editor like Editor, Wordpard etc...

Step 7: press the control and f key at the same time a similar window should show up that looks like this

![image](https://github.com/user-attachments/assets/3d955247-1e6d-47c8-98ee-eba2675295d0)

Step 8: Search for HideManagerGameObject in the "Search for" bar
 
Step 9: If set on false set it to true, you can leave the default value alone.

You're done

## How do I join rooms?
planning to add a new feature that makes you join random lobbies but you'll have to do this at the moment:

step 1: make sure your gamemode is selected  to "MODDED CASUAL" if not then set it to it with your VR headset.

step 2: generate a room with the "generate room" button.

step 3: use your freecam to move above the join trigger at stump(just like the added example)

![image](https://github.com/user-attachments/assets/de7ddeb5-66e6-4fbf-b5af-0c4e28131ccb)



step 4: press the "Leave Room" button.

step 5: Wait until you join a room and you're done.

# Known issues
Your skill if you don't use it

spams tons of NFE's(NullReferenceExpectations) errors into the console if not in a code.

send bug reports to my discord: ben.exe8137

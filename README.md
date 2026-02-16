# Intiface Game Vibration Router

[![Patreon donate button](https://img.shields.io/badge/patreon-donate-yellow.svg)](https://www.patreon.com/qdot)
[![Discourse Forum](https://img.shields.io/badge/discourse-forum-blue.svg)](https://discuss.buttplug.io)
[![Discord](https://img.shields.io/discord/353303527587708932.svg?logo=discord)](https://discord.buttplug.io)
[![Bluesky](https://img.shields.io/bluesky/followers/buttplug.io)](https://bsky.app/profile/buttplug.io)
[![Youtube](https://img.shields.io/badge/-youtube-red.svg)](https://youtube.buttplug.io)

The Intiface Game Haptics Router allows users to reroute vibration and other events from video games
to control sex hardware supported by [Buttplug and Intiface Central](https://intiface.com/central). This currently
includes:

- Games using Windows XInput or UWP (Xbox Gamepads)

The best place to find compatible games is the [The GHR Game Compatibility Thread](https://discuss.buttplug.io/t/game-haptics-router-compatibility-thread/74) thread on the Intiface/Buttplug forums.

Toys that support vibration or rotation are supported by the GHR:

- [List of supported vibrating toys](https://iostindex.com/?filter0Features=OutputsVibrators&filter1ButtplugSupport=4)
- [List of supported rotating toys](https://iostindex.com/?filter0Features=OutputsRotators&filter1ButtplugSupport=4)

Releases can be downloaded [on the releases page.](https://github.com/intiface/intiface-game-haptics-router/releases)

**[INTIFACE CENTRAL](https://intiface.com/central) is required to be installed for use with GHR.**

## Table Of Contents

- [Support The Project](#support-the-project)
- [FAQ](#faq)
- [License](#license)

## Support The Project

If you find this project helpful, you can [support us via Patreon](http://patreon.com/qdot)! Every
donation helps us afford more hardware to reverse, document, and write code for!

## How To Use

The following is a brief tutorial on usage of the Game Haptics Router (GHR). This should be enough to get you up and running with the system, or help troubleshoot any issues you may have.

### Getting Help If You Have Crashes/Issues

In order to inject rumble reroute functionality into games, the GHR does some very weird things that may make windows angry. This will lead to the GHR program crashing, buttons not working, etc.

If you are having problems with the GHR, the following resources are available for support:

- See the troubleshooting section at the end of this help document.
- [Threads on the Buttplug/Initface Forums](https://discuss.buttplug.io)  (No account required to view), including [The troubleshooting thread on the Buttplug/Intiface Forums](https://discuss.buttplug.io/t/troubleshooting-install-issues-with-the-game-haptics-router/73) and [The GHR Game Compatibility Thread](https://discuss.buttplug.io/t/game-haptics-router-compatibility-thread/74)
- [The GHR Channel on the Buttplug.io discord](https://discord.buttplug.io)

### The GHR Requires Intiface Central

As of v16, the GHR now requires [Intiface Central](https://intiface.com/central) to be up and running in order to access hardware. [Intiface Central](https://intiface.com/central) is the hub program for the Buttplug and Intiface Ecosystems, and contains all of the hardware connection/control functionality needed to make things like the GHR work. It's free, open source, and available on both desktop and mobile platforms.

### Using the GHR

The steps to using the GHR are as follows. If you run into any issues during these steps, see the Troubleshooting section below.

- Start Intiface Central, and hit the "Start Server" button
- Start the GHR, and make sure it's connected to Intiface Central.Intiface Central should show it as the currently connected client.
- From the GHR, hit "Start Scanning" to find your hardware
- Once your hardware is in the device list, click the checkbox next to it to make it active as a
  rumble rerouting target.
- Start the game you would like to reroute rumble from. This step can happen any time during the
  process, so it's not a problem if the game is already started before you start Intiface
  Central/GHR. The step is included at this point here for instruction clarity.
- Alt-tab back to the GHR, go to the "Process List" tab, and hit "Refresh List"
- If the process name of your game shows up in the list with "(XInput)" next to it, it means that it
  may be (but is not guaranteed to be) compatible with the GHR. Click on the process in the list and hit "Attach to Process"
- If all goes well, the "Attach to Process" button should be grayed out and the status message will
  read "Attached to Process".
- Alt-tab back to the game. Rumble should route from the game to the hardware you marked as active
  in the earlier steps.
- To see rumble signals coming from the game, check out the Visualizer panel. All rumble should show
  up on the graphs as events happen. If there is no activity on the graphs, see the troubleshooting section.

### Troubleshooting Common Issues

#### If the GHR or game crashes or won't start

[See the troubleshooting thread on the Buttplug/Intiface Forums](https://discuss.buttplug.io/t/troubleshooting-install-issues-with-the-game-haptics-router/73) (No account required to view).

#### Hardware related issues

If your toy isn't showing up on the device list or isn't reacting to rumble, it's best to make sure the toy is working with Intiface Central first.

- Close the game and the GHR
- Go to Intiface Central, make sure the server is started. If not, start it.
- Go to the "Devices" Tab of Intiface Central. Hit "Start Scanning", and make sure your device shows
  up. If your device shows up, move the related control sliders on the device panel to make sure that toy is working correctly with Intiface. If your device does not show up, there may be connection issues with Intiface. Contact support via one of the methods mentioned above.

If your hardware is visible and controllable through Intiface, see the next section for debugging issues in games.

#### No rumble rerouted from Game

If you know your hardware works but aren't getting rumble events from the game you're trying:

- Make sure the process is attached in the GHR
- Go to the visualizer screen and make sure events are shown in the graph whenever rumble happens
  in the game.

If no signals are showing up on the visualizer, there may be compatibility issues with the game. Either post on the discord or forums listed above to see if there are issues with games.

## FAQ

**How Finished is the GHR?**

The GHR is currently in *Beta* phase. This means it somewhat works. Should work with Xinput and UWP games.

**What Games does the GHR Work With?**

Technically, it should support any game that uses XInput or UWP Gamepad APIs.

That said, not all games are going to work with it in a way that is useful or fun.

See the games list above for a list of games tested with the software.

**How does the GHR work?**

We inject code into a running game process, find the rumble functions, and any time they are called,
forward the information out to the GHR, which then sends it to whatever hardware you've connected.

For XInput, we use [EasyHook](https://easyhook.github.io/) for attaching from managed C# to
unmanaged C.

**What kind of events does the GHR handle?**

Right now, any time a gamepad rumbles, we pass that information on to make hardware connected to the
GHR rumble.

In the future, we do plan on supporting game specific mods.

**Does the GHR require putting files in the game directory?**

No. All GHR mods are completely remote, require no file system writing, and only live for the
process lifetime. 

No one will know you put a buttplug in your game.

**Can I use the GHR with games with anti-cheat mechanisms?**

No. Our injection and loading process, while usually not VAC triggering, will still be caught by
games like Overwatch, Rocket League, etc... and denied. We do not recommend using the GHR with any
game that has anti-cheat mechanisms, and we are not resposible for whatever may happen to your game
account if you try it.

**What type of hardware does the GHR work with?**

At the moment, only vibrating toys, as listed on the bottom of the front page at
[https://buttplug.io]. 

Support for thrusting/stroking/rotating toys will be available in a future release. See [this
issue](https://github.com/intiface/intiface-game-haptics-router/issues/1) for more information.

**What's the Intiface Panel do?**

The Intiface Panel is how we deal with connecting to supported hardware. Users are required to use [Intiface Central](https://intiface.com/central)

## License

The Intiface Game Haptics Router is BSD 3-Clause licensed. More information is available in the LICENSE file.


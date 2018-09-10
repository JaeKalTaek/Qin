/* -------------------------------------------
   FMOD Studio Script Example:
   Randomize Colors
   -------------------------------------------
 */

studio.menu.addMenuItem({ name: "FMOD Examples\\Randomize Colors",
	isEnabled: function() { var event = studio.window.browserCurrent(); return event && event.isOfExactType("Event"); },
	keySequence: "Alt+R",
	execute: function() {
		var colors = ["Blue", "Cyan", "Green", "Magenta", "Red", "Yellow"];
		var event = studio.window.browserCurrent();
		event.groupTracks.forEach(function(track) {
			var randomColor = colors[Math.floor(Math.random() * colors.length)];
			track.mixerGroup.color = randomColor;
			track.modules.forEach(function(module) {
				module.color = randomColor;
			});
		});
	}
});
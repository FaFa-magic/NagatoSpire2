// Run once with fmodstudiocl -script create-events.js Nagato.fspro.
// The project retains the game's mixer GUIDs but publishes only Nagato.bank.
if (studio.project.lookup("bank:/Nagato"))
    throw new Error("Nagato events already exist; rebuild with build-bank.ps1 instead.");
var bank = studio.project.create("Bank");
bank.name = "Nagato";
bank.folder = studio.project.workspace.masterBankFolder;
var folder = studio.project.create("EventFolder");
folder.name = "nagato";
folder.folder = studio.project.lookup("event:/sfx");
var files = [
    ["attack", "Nagato_attacksfx.mp3"],
    ["cast", "Nagato_castsfx.mp3"],
    ["death", "Nagato_deathsfx.mp3"],
    ["character_select", "Nagato_character_select.mp3"],
    ["character_transition", "Nagato_character_transition.mp3"],
    ["combat_start", "Nagato_opensfx.mp3"],
    ["orb_passive", "tashkent_PassiveSfx.mp3"],
    ["orb_evoke", "tashkent_EvokeSfx.mp3"],
    ["orb_channel", "tashkent_ChannelSfx.mp3"]
];
for (var i = 0; i < files.length; ++i) {
    var sourceRoot = studio.project.filePath.replace(/[^\/\\]+$/, "") + "../NagatoSpire2/sfx/";
    var asset = studio.project.importAudioFile(sourceRoot + files[i][1]);
    if (!asset) throw new Error("Could not import " + files[i][1]);
    var event = studio.project.workspace.addEvent(files[i][0], false);
    event.folder = folder;
    event.relationships.banks.add(bank);
    event.mixerInput.output = studio.project.lookup("bus:/master/sfx");
    // Unity gain; the game's SFX/master buses control playback volume.
    event.mixer.masterBus.volume = 0;
    var track = event.addGroupTrack("Voice");
    var sound = track.addSound(event.timeline, "SingleSound", 0, asset.length);
    sound.audioFile = asset;
    console.log("Created " + files[i][0] + " (" + asset.length + " s)");
}
console.log("Saving Nagato project");
if (!studio.project.save()) throw new Error("Could not save Nagato FMOD project");

// apt install fswebcam
// npm i node-webcam

//Available in nodejs

const NodeWebcam = require('node-webcam');

//Default options

const opts = {
    //Picture related
    width: 1280,
    height: 720,
    quality: 100,

    // Number of frames to capture
    // More the frames, longer it takes to capture
    // Use higher framerate for quality. Ex: 60
    frames: 60,

    //Delay in seconds to take shot
    //if the platform supports miliseconds
    //use a float (0.1)
    //Currently only on windows
    delay: 0,

    //Save shots in memory
    saveShots: true,

    // [jpeg, png] support varies
    // Webcam.OutputTypes
    output: 'jpeg',

    //Which camera to use
    //Use Webcam.list() for results
    //false for default device
    device: false,

    // [location, buffer, base64]
    // Webcam.CallbackReturnTypes
    callbackReturn: 'base64',

    //Logging
    verbose: false
};

//Creates webcam instance
// var Webcam = NodeWebcam.create(opts);
const Webcam = NodeWebcam.Factory.create(opts, 'fswebcam');

//Will automatically append location output type
Webcam.capture(null, (err, data) => {
    // Permission denied: G1szMW1zdGF0OiBQZXJtaXNzaW9uIGRlbmllZAobWzBt
    console.log('--------Error--------');
    console.log(err);
    console.log('--------Data--------');
    console.log(data);
});
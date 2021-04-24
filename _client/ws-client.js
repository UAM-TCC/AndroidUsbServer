const WebSocket = require('ws');

const client = new WebSocket('ws://192.168.0.5:8000');

let state = false;
const changeState = () => { state = !state; return state ? '1' : '0' }

// client.on('open', () => client.send(changeState()));
client.on('open', () => setInterval(() => client.send(changeState()), 1000));

client.on('message', data => console.log(data));

client.on('close', (code, reason) => { console.log(code, reason); exitHandler({exit:true}); })

client.on('error', err => console.error(err));

setTimeout(() => client.close(), 10000);

function exitHandler(options, exitCode) {
    client.close();
    if (options.exit) process.exit();
}

//do something when app is closing
process.on('exit', exitHandler.bind(null, {cleanup:true}));

//catches ctrl+c event
process.on('SIGINT', exitHandler.bind(null, {exit:true}));

// catches "kill pid" (for example: nodemon restart)
process.on('SIGUSR1', exitHandler.bind(null, {exit:true}));
process.on('SIGUSR2', exitHandler.bind(null, {exit:true}));

//catches uncaught exceptions
process.on('uncaughtException', exitHandler.bind(null, {exit:true}));
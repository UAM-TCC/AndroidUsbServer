const WebSocket = require('ws');

const client = new WebSocket(`ws://${process.argv[2]}`);

let state = false;
const changeState = () => { state = !state; return state ? 'ON' : 'OFF' }

client.on('open', () => setInterval(() => client.send(changeState()), 1000));
client.on('message', data => console.log('>', data));
client.on('close', (code, reason) => console.log('Closing...', code, reason));
client.on('error', err => console.error(err));

setTimeout(() => client.close(), 10000);
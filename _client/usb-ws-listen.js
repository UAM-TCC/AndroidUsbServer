const WebSocket = require('ws');

const client = new WebSocket(`ws://${process.argv[2]}`);

client.on('open', () => console.log('Listening...'));
client.on('message', data => console.log('>', data));
client.on('close', (code, reason) => console.log('Closing...', code, reason));
client.on('error', err => console.error(err));
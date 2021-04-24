var net = require('net');

var client = new net.Socket();
client.connect(8000, '192.168.0.5', function() {
	console.log('Connected');
	client.write(process.argv[2]); // ON / OFF
});

client.on('data', function(data) {
	console.log('Received: ' + data);
	client.destroy(); // kill client after server's response
});

client.on('close', function() {
	console.log('Connection closed');
});
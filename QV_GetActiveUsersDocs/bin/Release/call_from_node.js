var spawn = require('child_process').spawn;
var child = spawn('QV_GetActiveUsersDocs', ['-f', 'json', '-a', 'false']);

child.stdout.on('data', function(chunk) {
  var output = chunk.toString();

  try {
	output = JSON.parse(output);
    var msg = output.msg;
	console.log(msg.text);
  } catch (err) {    
    console.log(output)
  }
});
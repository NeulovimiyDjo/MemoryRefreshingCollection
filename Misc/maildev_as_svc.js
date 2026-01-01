// npm install -g node-windows

// ##cd some/dir/for/installation
// npm install maildev
// npm link node-windows

// ##copy maildev_as_svc.js to some/dir/for/installation
// ##start cmd as administrator
// node ./maildev_as_svc.js


var Service = require('node-windows').Service;

var svc = new Service({
  name: 'maildev', // This is a display name. Service name is maildev.exe
  description: 'maildev server.',
  script: require('path').join(__dirname, 'node_modules/maildev/bin/maildev'),
  scriptOptions: '-s 5025 -w 5080'
});

svc.on('install', function() {
  console.log('Install complete.');
  console.log('The service exists: ', svc.exists);
  svc.start();
});

svc.on('alreadyinstalled', function() {
  console.log('Already installed.');
  console.log('The service exists: ', svc.exists);
});

svc.on('invalidinstallation', function() {
  console.log('Invalid installation.');
  console.log('The service exists: ', svc.exists);
});

svc.on('uninstall', function() {
  console.log('Uninstall complete.');
  console.log('The service exists: ', svc.exists);
});

svc.on('alreadyuninstalled', function() {
  console.log('Already uninstalled.');
  console.log('The service exists: ', svc.exists);
});

svc.on('error', function() {
  console.log('Error.');
  console.log('The service exists: ', svc.exists);
});

svc.uninstall();
svc.install();

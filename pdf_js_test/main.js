//'use strict';

var fs = require('fs');

// HACK few hacks to let PDF.js be loaded not as a module in global space.
global.window = global;
global.navigator = { userAgent: 'node' };
global.DOMParser = require('./domparsemock.js').DOMParserMock; // https://github.com/mozilla/pdf.js/blob/master/examples/node/domparsermock.js
require('./domstubs.js'); // https://github.com/mozilla/pdf.js/blob/master/examples/node/domstubs.js
global.PDFJS = {};
PDFJS.workerSrc = true;

require('./node_modules/pdfjs-dist/build/pdf.combined.js');

var pdfPath = '/Users/kkowalczyk/Downloads/pdfs/Seth-Godins-Startup-School.pdf';
var data = new Uint8Array(fs.readFileSync(pdfPath));
console.log("file size: " + data.length);

PDFJS.getDocument(data).then(function (doc) {
  var numPages = doc.numPages;
  console.log('# Document Loaded');
  console.log('Number of Pages: ' + numPages);
  console.log();

  var lastPromise; // will be used to chain promises

  lastPromise = doc.getMetadata().then(function (data) {
    console.log('# Metadata Is Loaded');
    console.log('## Info');
    console.log(JSON.stringify(data.info, null, 2));
    console.log();
    if (data.metadata) {
      console.log('## Metadata');
      console.log(JSON.stringify(data.metadata.metadata, null, 2));
      console.log();
    }
  });

  var loadPage = function (pageNum) {
    return doc.getPage(pageNum).then(function (page) {
      console.log('Page: ' + pageNum);
      var viewport = page.getViewport(1.0 /* scale */);
      console.log('  Size: ' + viewport.width + 'x' + viewport.height);
    });
  };

  for (var i = 1; i <= numPages; i++) {
    lastPromise = lastPromise.then(loadPage.bind(null, i));
  }
  return lastPromise;
}).then(function () {
  console.log('# End of Document');
}, function (err) {
  console.error('Error: ' + err);
});

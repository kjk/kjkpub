function genHtml(files) {
  // files is [] where each element is: [type, rest]
  // Types:
  // 'a': added    file, rest is: filePath, idOfContent
  // 'd': deleted  file, rest is: filePath, idOfContent
  // 'm': modified file, rest is: filePath, idOfContentBefore, idOfContentAfter
  // 'u': not versined,  rest is: filePath, idOfContent
  console.log("genHtml()", files);
}

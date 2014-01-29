/*
You can use this script on the duo lingo vocabulary page to scrape knows words and their strengths.
1/25/2014
*/


var rows = $('.first-lemma-row')
var output = "";
rows.each(function (i, row) {
    var word = $('.lemma', $(row)).text();
    var wordStrength = $('.filled', $(row)).length;
    output += word + "," + wordStrength + ";";
});
console.log(output);
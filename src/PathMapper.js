'use strict';

const fs = require('fs');
const path = require('path');

const rejectedDirectories = ['.git', 'node_modules', '.vscode', '.idea', '.gitignore'];

class PathMapper {
    constructor() {
        this.path = __dirname;
        this.mapDirectory(__dirname, this);
    }

    mapDirectory(directory, currentObj) {
        const files = fs.readdirSync(directory);

        files.forEach(filename => {
            const filePath = path.parse(path.join(directory, filename));
            if (filePath.name === 'path') throw new Error('No folder can be named path.');
            if (filePath.ext === '' && !rejectedDirectories.includes(filePath.name)) {
                const fullPath = path.format(filePath); 
                currentObj[filePath.name] = {};                           
                currentObj[filePath.name].path = fullPath;
                this.mapDirectory(fullPath, currentObj[filePath.name]);
            }
        });
    }
}

module.exports = new PathMapper();
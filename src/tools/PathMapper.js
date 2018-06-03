'use strict';

const fs = require('fs');
const path = require('path');

const rejectedDirectories = ['.git', 'node_modules', '.vscode', '.idea', '.gitignore'];

class PathMapper {
    constructor(initDirectory) {
        const initDirectoryName = path.parse(initDirectory).name;
        if (rejectedDirectories.includes(initDirectoryName))
            throw new Error('The initial directory name is in the rejected directories list.');

        this.path = initDirectory;
        this.mapDirectory(initDirectory, this);
    }

    mapDirectory(directory, currentObj) {
        const files = fs.readdirSync(directory);

        files.forEach(filename => {
            const filePath = path.parse(path.join(directory, filename));
            if (filePath.name === 'path') throw new Error('No folder can be named path.');
            if (filePath.ext === '' && !rejectedDirectories.includes(filePath.name)) {
                const fullPath = path.format(filePath);

                currentObj[filePath.name] = {
                    'path': fullPath
                };

                this.mapDirectory(fullPath, currentObj[filePath.name]);
            }
        });
    }
}

module.exports = PathMapper;
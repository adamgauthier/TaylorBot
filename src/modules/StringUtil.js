'use strict';

class StringUtil {
    static shrinkString(str, charLimit, adder = '', shrinkAfterChars = ['\n', '.', ' ']) {
        if (str.length <= charLimit)
            return str;

        charLimit -= adder.length;
        let substringTest = str.substring(0, charLimit);

        let lastIndex = -1;

        shrinkAfterChars.forEach(char => {
            const currentLastIndex = substringTest.lastIndexOf(char);
            if (currentLastIndex > lastIndex)
                lastIndex = currentLastIndex;
        });

        if (lastIndex === -1) lastIndex = charLimit;

        return str.substring(0, lastIndex) + adder;
    }

    static plural(size, itemName, between = ' ') {
        const isPlural = size > 1;

        if (isPlural) {
            if (itemName.endsWith('y'))
                itemName = `${itemName.slice(0, -1)}ies`;
            else
                itemName = `${itemName}s`;
        }

        return `${size}${between}${itemName}`;
    }
}

module.exports = StringUtil;
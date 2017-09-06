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
}

module.exports = StringUtil;
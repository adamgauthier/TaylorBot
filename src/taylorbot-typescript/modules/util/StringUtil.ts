export class StringUtil {
    static shrinkString(str: string, charLimit: number, adder = '', shrinkAfterChars = ['\n', '.', ' ']): string {
        if (str.length <= charLimit)
            return str;

        charLimit -= adder.length;
        const substringTest = str.substring(0, charLimit);

        let lastIndex = -1;

        shrinkAfterChars.forEach(char => {
            const currentLastIndex = substringTest.lastIndexOf(char);
            if (currentLastIndex > lastIndex)
                lastIndex = currentLastIndex;
        });

        if (lastIndex === -1) lastIndex = charLimit;

        return str.substring(0, lastIndex) + adder;
    }

    static plural(size: number | string | BigInt, itemName: string, surround = '', useS = false): string {
        const isPlural = size !== 1 && size !== BigInt(1) && size !== '1';

        if (isPlural) {
            if (!useS && itemName.endsWith('y'))
                itemName = `${itemName.slice(0, -1)}ies`;
            else
                itemName = `${itemName}s`;
        }

        return `${surround}${StringUtil.formatNumberString(size)}${surround} ${itemName}`;
    }

    static formatNumberString(numberAsString: { toString: () => string }): string {
        return numberAsString.toString().replace(/(\d)(?=(\d{3})+(?!\d))/g, '$1,');
    }
}

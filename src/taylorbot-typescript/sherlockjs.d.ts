declare module 'sherlockjs' {
    export type SherlockResult = {
        eventTitle: string;
        startDate: Date | null;
        endDate: Date;
        isAllDay: boolean;
    };

    export function parse(str: string): SherlockResult;
}

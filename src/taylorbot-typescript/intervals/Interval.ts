import { TaylorBotClient } from '../client/TaylorBotClient';

export abstract class Interval {
    id: string;
    intervalMs: number;
    enabled: boolean;
    runningInterval: NodeJS.Timeout | null = null;

    constructor({ id, intervalMs, enabled = true }: { id: string; intervalMs: number; enabled?: boolean }) {
        this.id = id;
        this.intervalMs = intervalMs;
        this.enabled = enabled;
    }

    abstract interval(client: TaylorBotClient): Promise<void>;
}

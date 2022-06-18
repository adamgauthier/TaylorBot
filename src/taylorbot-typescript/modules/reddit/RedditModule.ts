type SubredditAboutResponse = {
    error: number | undefined;
    reason: string | undefined;
    data: {
        title: string;
        active_user_count: number;
        icon_img: string;
        display_name_prefixed: string;
        subscribers: number;
        public_description: string;
        key_color: string;
        created: number;
    };
};

export class RedditModule {
    static async getSubredditAbout(subreddit: string): Promise<SubredditAboutResponse> {
        return (await fetch(
            `https://www.reddit.com/r/${subreddit}/about/.json`
        ).then(res => res.json())) as SubredditAboutResponse;
    }
}

module.exports = {
    MemberOrAuthor: {
        prompt: 'What user would you like to see the info of?',
        type: 'member',
        default: message => message.member,
        error: 'Could not find user'
    }
};
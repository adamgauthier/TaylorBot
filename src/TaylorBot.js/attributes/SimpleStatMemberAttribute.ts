import { MemberAttribute, MemberAttributeParameters } from './MemberAttribute';

export type SimpleStatMemberAttributeParameters = MemberAttributeParameters & { singularName: string };

export abstract class SimpleStatMemberAttribute extends MemberAttribute {
    singularName: string;

    constructor(options: SimpleStatMemberAttributeParameters) {
        super(options);
        this.singularName = options.singularName;
    }
}

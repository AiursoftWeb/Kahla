export interface ThreadOptions {
    name: string;
    allowSearchByName: boolean;
    allowDirectJoinWithoutInvitation: boolean;
    allowMemberSoftInvitation: boolean;
    allowMembersSendMessages: boolean;
    allowMembersEnlistAllMembers: boolean;

    iconFilePath?: string; // Not required for create stratch.
}

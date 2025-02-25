import { ThreadMemberInfo } from './ThreadMemberInfo';

export interface ThreadMembersApiResponse {
    members: ThreadMemberInfo[];
    totalCount: number;
}

import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent {
  members$: Observable<Member[]> | undefined;

  constructor(private membersServices:MembersService) { }

  ngOnInit(): void {
    this.members$ = this.membersServices.getMembers();
  }
}

import { Component, OnInit } from '@angular/core';

import * as d3 from 'd3';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  private nodes: any[];
  private simulation: any;
  private links: any[];

  private canvas: HTMLCanvasElement;
  private context: CanvasRenderingContext2D;

  private width: number;
  private height: number;

  ngOnInit(): void {
    this.initCanvas();
    this.initNodes();
    this.initSimulation();
  }

  private initCanvas() {
    this.canvas = document.querySelector('canvas');
    this.context = this.canvas.getContext('2d');
    this.width = this.canvas.width;
    this.height = this.canvas.height;
  }

  private initNodes() {
    this.nodes = [
      { id: 'A', name: 'A node', reflexive: false, group: 1 },
      { id: 'B', name: 'B node', reflexive: false, group: 1 },
      { id: 'C', name: 'C node', reflexive: false, group: 1 }
    ];

    this.links = [
      { source: this.nodes[0].id, target: this.nodes[1].id, left: false, right: true },
      { source: this.nodes[1].id, target: this.nodes[2].id, left: false, right: true }
    ];
  }

  private initSimulation() {
    this.simulation = d3.forceSimulation()
      .force('link', d3.forceLink()
        .id(function (link) { return link['id']; })
        .distance(function (link) { return 100; })
        .strength(1.5)
      )
      .force('charge', d3.forceManyBody())
      .force('center', d3.forceCenter(this.width / 2, this.height / 2))
      .nodes(this.nodes)
      .on('tick', () => this.ticked());

    this.simulation.force('link')
      .links(this.links);

    d3.select(this.canvas)
      .call(d3.drag()
        .container(this.canvas)
        .subject(() => this.dragSubject())
        .on('start', () => this.onDragStart())
        .on('drag', () => this.onDrag())
        .on('end', () => this.onDragEnd())
      );
  }

  private dragSubject(): any {
    return this.simulation.find(d3.event.x, d3.event.y);
  }

  private onDragStart() {
    if (!d3.event.active) {
      this.simulation.alphaTarget(0.3).restart();
    }

    d3.event.subject.fx = d3.event.subject.x;
    d3.event.subject.fy = d3.event.subject.y;
  }

  private onDrag() {
    d3.event.subject.fx = d3.event.x;
    d3.event.subject.fy = d3.event.y;
  }

  private onDragEnd() {
    if (!d3.event.active) { this.simulation.alphaTarget(0); }
    d3.event.subject.fx = null;
    d3.event.subject.fy = null;
  }

  private ticked() {
    this.context.clearRect(0, 0, this.width, this.height);
    this.context.save();

    this.context.beginPath();
    this.links.forEach((link) => this.drawLink(link));
    this.context.strokeStyle = '#aaa';
    this.context.stroke();

    this.context.beginPath();
    this.nodes.forEach((link) => this.drawNode(link));
    this.context.fillStyle = 'blue';
    this.context.fill();

    this.context.restore();
  }

  private drawLink(link) {
    this.context.moveTo(link.source.x, link.source.y);
    this.context.lineTo(link.target.x, link.target.y);
  }

  private drawNode(node) {
    this.context.moveTo(node.x + 3, node.y);
    this.context.arc(node.x, node.y, 10, 0, 2 * Math.PI);

    const textWidth = this.context.measureText(node.name).width;
    // this is a GUESS of height, just taking a single character to figure things out
    const textHeight = this.context.measureText('w').width;
    this.context.fillText(node.name, node.x - (textWidth / 2), node.y - textHeight - 10);
  }
}
